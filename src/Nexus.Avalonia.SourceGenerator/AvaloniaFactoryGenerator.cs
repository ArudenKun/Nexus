using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Nexus.Avalonia.SourceGenerator;

[Generator]
public class AvaloniaFactoryGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName =
        "Avalonia.Markup.Declarative.GenerateFactoriesForAssemblyAttribute";

    private const string AttributeMetadataContent = """

        namespace Avalonia.Markup.Declarative
        {
            using System;

            /// <summary>
            /// Opts an external library in so that its public controls are also exposed
            /// through <c>Avalonia.Markup.Declarative.Factories</c>.
            /// Base Avalonia controls and types in the current project are always included;
            /// you only need this attribute for third-party assemblies.
            /// </summary>
            [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class GenerateFactoriesForAssemblyAttribute : Attribute
            {
                public GenerateFactoriesForAssemblyAttribute(Type anchorType)
                {
                    AnchorType = anchorType;
                }

                /// <summary>The type used to locate the assembly to scan for controls.</summary>
                public Type AnchorType { get; }
            }
        }
        """;

    private const string ControlMetadataName = "Avalonia.Controls.Control";
    private const string StyledElementMetadataName = "Avalonia.StyledElement";
    private const string AvaloniaAssemblyName = "Avalonia.Controls";

    /// <summary>
    /// A batch of controls emitted together into a single generated file.
    /// <see cref="ExternalAssemblyName"/> is null for the core batch
    /// (Avalonia framework + current project); non-null for opted-in externals.
    /// </summary>
    private sealed record FactoryGroup(
        string? ExternalAssemblyName,
        ImmutableArray<INamedTypeSymbol> Controls
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postContext =>
        {
            postContext.AddSource($"{AttributeMetadataName}.g.cs", AttributeMetadataContent);
        });

        var groupsProvider = context.CompilationProvider.Select(
            (compilation, _) =>
            {
                var controlSymbol = compilation.GetTypeByMetadataName(ControlMetadataName);
                var styledElementSymbol = compilation.GetTypeByMetadataName(
                    StyledElementMetadataName
                );

                // Bail early if Avalonia isn't referenced at all.
                if (controlSymbol is null && styledElementSymbol is null)
                    return ImmutableArray<FactoryGroup>.Empty;

                var seen = new HashSet<string>();
                var groups = ImmutableArray.CreateBuilder<FactoryGroup>();

                void TryAdd(List<INamedTypeSymbol> bag, INamedTypeSymbol type)
                {
                    if (!IsEligibleControl(type, controlSymbol, styledElementSymbol))
                        return;

                    var key = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (!seen.Add(key))
                        return;

                    bag.Add(type);
                }

                // ── (1) CORE: Avalonia framework + current project ──────────
                var coreControls = new List<INamedTypeSymbol>();

                var avaloniaAssemblies = compilation
                    .SourceModule.ReferencedAssemblySymbols.Where(a =>
                        a.Name == AvaloniaAssemblyName || a.Name.StartsWith("Avalonia.")
                    )
                    .ToList();

                foreach (var asm in avaloniaAssemblies)
                foreach (var type in GetNamespaceTypes(asm.GlobalNamespace))
                    TryAdd(coreControls, type);

                foreach (var type in GetNamespaceTypes(compilation.Assembly.GlobalNamespace))
                    TryAdd(coreControls, type);

                if (coreControls.Count > 0)
                    groups.Add(new FactoryGroup(null, [.. coreControls]));

                // ── (2) EXTERNAL: per-assembly opt-in via the attribute ─────
                var attributeSymbol = compilation.GetTypeByMetadataName(AttributeMetadataName);
                if (attributeSymbol is not null)
                {
                    // Group anchors by containing assembly so multiple attributes
                    // anchored to the same library produce exactly one file.
                    var byAssembly =
                        new Dictionary<string, (IAssemblySymbol Asm, List<INamedTypeSymbol> Bag)>();

                    foreach (var attr in compilation.Assembly.GetAttributes())
                    {
                        if (
                            !SymbolEqualityComparer.Default.Equals(
                                attr.AttributeClass,
                                attributeSymbol
                            )
                        )
                            continue;

                        if (attr.ConstructorArguments.Length < 1)
                            continue;

                        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol anchor)
                            continue;

                        var asm = anchor.ContainingAssembly;
                        if (asm is null)
                            continue;

                        var asmName = asm.Name;
                        if (!byAssembly.TryGetValue(asmName, out var entry))
                        {
                            entry = (asm, new List<INamedTypeSymbol>());
                            byAssembly[asmName] = entry;
                        }

                        foreach (var type in GetNamespaceTypes(asm.GlobalNamespace))
                            TryAdd(entry.Bag, type);
                    }

                    foreach (var kv in byAssembly.OrderBy(k => k.Key, StringComparer.Ordinal))
                    {
                        if (kv.Value.Bag.Count == 0)
                            continue;
                        groups.Add(new FactoryGroup(kv.Key, [.. kv.Value.Bag]));
                    }
                }

                return groups.ToImmutable();
            }
        );

        context.RegisterSourceOutput(groupsProvider, (spc, groups) =>
        {
            if (groups.IsDefaultOrEmpty)
                return;

            foreach (var group in groups)
            {
                var ordered = group
                    .Controls.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.ContainingNamespace.ToDisplayString(), StringComparer.Ordinal)
                    .ToList();

                var fileName = group.ExternalAssemblyName is null
                    ? "Avalonia.Markup.Declarative.Factories.g.cs"
                    : $"Avalonia.Markup.Declarative.Factories.External.{SanitizeFileName(group.ExternalAssemblyName)}.g.cs";

                Emit(spc, fileName, ordered);
            }
        });
    }

    private static void Emit(
        SourceProductionContext spc,
        string fileName,
        List<INamedTypeSymbol> ordered
    )
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace Avalonia.Markup.Declarative");
        sb.AppendLine("{");
        sb.AppendLine("    public static partial class Factories");
        sb.AppendLine("    {");

        foreach (var control in ordered)
        {
            var shortName = control.Name;
            var fullName = control.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            sb.AppendLine($"        /// <summary>Creates a new instance of {fullName}.</summary>");
            sb.AppendLine($"        public static {fullName} {shortName}() => new {fullName}();");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        spc.AddSource(fileName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    // Replace any character not legal in a Roslyn source hint name.
    private static string SanitizeFileName(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var ch in name)
            sb.Append(char.IsLetterOrDigit(ch) || ch == '_' || ch == '.' ? ch : '_');
        return sb.ToString();
    }

    private static bool IsEligibleControl(
        INamedTypeSymbol type,
        INamedTypeSymbol? controlSymbol,
        INamedTypeSymbol? styledElementSymbol
    )
    {
        if (type.TypeKind != TypeKind.Class)
            return false;
        if (type.IsAbstract || type.IsStatic)
            return false;
        if (type.IsGenericType)
            return false;
        if (type.DeclaredAccessibility != Accessibility.Public)
            return false;

        // Must have a public parameterless constructor to be instantiable via a factory.
        if (
            !type.InstanceConstructors.Any(c =>
                c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public
            )
        )
            return false;

        return InheritsFrom(type, controlSymbol) || InheritsFrom(type, styledElementSymbol);
    }

    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol? baseType)
    {
        if (baseType is null)
            return false;
        var current = type.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;
            current = current.BaseType;
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetNamespaceTypes(INamespaceSymbol ns)
    {
        foreach (var type in ns.GetTypeMembers())
            yield return type;

        foreach (var child in ns.GetNamespaceMembers())
        foreach (var type in GetNamespaceTypes(child))
            yield return type;
    }
}
