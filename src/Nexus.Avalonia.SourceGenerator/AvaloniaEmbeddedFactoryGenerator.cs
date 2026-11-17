using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Nexus.Avalonia.SourceGenerator;

// [Generator]
public class AvaloniaEmbeddedFactoryGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName =
        "Avalonia.Markup.Declarative.GenerateFactoriesForAssemblyAttribute";

    private const string ControlMetadataName = "Avalonia.Controls.Control";
    private const string StyledElementMetadataName = "Avalonia.StyledElement";

    private const string DeclarativeViewBaseName =
        "Avalonia.Markup.Declarative.IDeclarativeViewBase";

    private const string AvaloniaAssemblyName = "Avalonia.Controls";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // ── Build the eligible-control list, grouped by source assembly ─────
        var eligibleControls = context.CompilationProvider.Select(
            (compilation, _) =>
            {
                var controlSymbol = compilation.GetTypeByMetadataName(ControlMetadataName);
                var styledElementSymbol = compilation.GetTypeByMetadataName(
                    StyledElementMetadataName
                );

                if (controlSymbol is null && styledElementSymbol is null)
                    return ImmutableArray<ControlGroup>.Empty;

                var seen = new HashSet<string>();
                var groups = new List<ControlGroup>();

                // (1) CORE: Avalonia framework controls.
                var coreControls = new List<ControlInfo>();
                foreach (
                    var asm in compilation.SourceModule.ReferencedAssemblySymbols.Where(a =>
                        a.Name == AvaloniaAssemblyName || a.Name.StartsWith("Avalonia.")
                    )
                )
                {
                    foreach (var type in GetNamespaceTypes(asm.GlobalNamespace))
                        TryAdd(coreControls, type);
                }

                if (coreControls.Count > 0)
                    groups.Add(new ControlGroup("Avalonia", coreControls));

                // (2) OPT-IN: external assemblies via attribute, one group per assembly.
                var attributeSymbol = compilation.GetTypeByMetadataName(AttributeMetadataName);
                if (attributeSymbol is not null)
                {
                    var byAssembly = new Dictionary<string, List<ControlInfo>>(
                        StringComparer.Ordinal
                    );

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

                        if (!byAssembly.TryGetValue(asm.Name, out var bucket))
                        {
                            bucket = new List<ControlInfo>();
                            byAssembly[asm.Name] = bucket;
                        }

                        foreach (var type in GetNamespaceTypes(asm.GlobalNamespace))
                            TryAdd(bucket, type);
                    }

                    foreach (var kv in byAssembly.OrderBy(k => k.Key, StringComparer.Ordinal))
                    {
                        if (kv.Value.Count == 0)
                            continue;
                        groups.Add(new ControlGroup(kv.Key, kv.Value));
                    }
                }

                return groups.ToImmutableArray();

                // Local function captures the dedupe set and eligibility check.
                void TryAdd(List<ControlInfo> bag, INamedTypeSymbol type)
                {
                    if (!IsEligibleControl(type, controlSymbol, styledElementSymbol))
                        return;

                    var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (!seen.Add(fullName))
                        return;

                    bag.Add(new ControlInfo(type.Name, fullName));
                }
            }
        );

        // ── Discover partial classes in the current project ─────────────────
        var views = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds
                    && cds.Modifiers.Any(m => m.ValueText == "partial"),
                transform: static (ctx, ct) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(
                        (ClassDeclarationSyntax)ctx.Node,
                        ct
                    );
                    return symbol;
                }
            )
            .Where(static s => s is not null)
            .Select(static (s, _) => s!);

        // ── Combine views + controls + compilation ──────────────────────────
        var combined = views
            .Collect()
            .Combine(eligibleControls)
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, (spc, tuple) =>
        {
            var ((innerViews, controlGroups), compilation) = tuple;

            if (controlGroups.IsDefaultOrEmpty || innerViews.IsDefaultOrEmpty)
                return;

            var declarativeViewBaseSymbol = compilation.GetTypeByMetadataName(
                DeclarativeViewBaseName
            );
            if (declarativeViewBaseSymbol is null)
                return; // Avalonia.Markup.Declarative not referenced.

            var seenViews = new HashSet<string>(StringComparer.Ordinal);

            foreach (var view in innerViews)
            {
                if (view is null)
                    continue;
                if (view.TypeKind != TypeKind.Class)
                    continue;
                if (view.IsAbstract)
                    continue;
                if (
                    view.DeclaredAccessibility != Accessibility.Public
                    && view.DeclaredAccessibility != Accessibility.Internal
                )
                    continue;

                if (!ImplementsDeclarativeViewBase(view, declarativeViewBaseSymbol))
                    continue;

                var key = view.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (!seenViews.Add(key))
                    continue;

                EmitView(spc, view, controlGroups);
            }
        });
    }

    /// <summary>
    /// True if <paramref name="view"/> (or any base type) implements
    /// <c>IDeclarativeViewBase</c>. Because ViewBase itself implements the
    /// interface, this single check covers ViewBase, ViewBase&lt;T&gt;, and any
    /// custom base that opts into the same contract.
    /// </summary>
    private static bool ImplementsDeclarativeViewBase(
        INamedTypeSymbol view,
        INamedTypeSymbol declarativeViewBase
    )
    {
        foreach (var iface in view.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, declarativeViewBase))
                return true;
        }

        return false;
    }

    private static void EmitView(
        SourceProductionContext spc,
        INamedTypeSymbol view,
        ImmutableArray<ControlGroup> controlGroups
    )
    {
        // Skip any control whose method name already exists on the view.
        var existingMembers = new HashSet<string>(StringComparer.Ordinal);
        for (var t = view; t is not null; t = t.BaseType)
            foreach (var m in t.GetMembers())
                existingMembers.Add(m.Name);

        // Filter and order per group; drop groups that end up empty.
        var filteredGroups = controlGroups
            .Select(g => new ControlGroup(
                g.AssemblyName,
                g.Controls.Where(c => !existingMembers.Contains(c.MethodName))
                    .OrderBy(c => c.MethodName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(c => c.FullName, StringComparer.Ordinal)
                    .ToList()
            ))
            .Where(g => g.Controls.Count > 0)
            .ToList();

        if (filteredGroups.Count == 0)
            return;

        var ns = view.ContainingNamespace.IsGlobalNamespace
            ? null
            : view.ContainingNamespace.ToDisplayString();
        var className = view.Name;
        var typeParamList =
            view.TypeParameters.Length > 0
                ? "<" + string.Join(", ", view.TypeParameters.Select(p => p.Name)) + ">"
                : string.Empty;
        var containingPath = BuildContainingTypePath(view);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        if (ns is not null)
        {
            sb.Append("namespace ").AppendLine(ns);
            sb.AppendLine("{");
        }

        // Re-open every containing partial class (nested views).
        var indent = ns is not null ? 1 : 0;
        foreach (var wrapper in containingPath)
        {
            sb.Append(new string(' ', indent * 4))
                .Append(wrapper.Accessibility)
                .Append(" partial class ")
                .Append(wrapper.Name)
                .Append(wrapper.TypeParams)
                .AppendLine();
            sb.Append(new string(' ', indent * 4)).AppendLine("{");
            indent++;
        }

        var classIndent = new string(' ', indent * 4);
        sb.Append(classIndent)
            .Append(view.DeclaredAccessibility == Accessibility.Public ? "public" : "internal")
            .Append(" partial class ")
            .Append(className)
            .Append(typeParamList)
            .AppendLine();
        sb.Append(classIndent).AppendLine("{");

        var regionIndent = new string(' ', (indent + 1) * 4);
        var methodIndent = new string(' ', (indent + 2) * 4);

        foreach (var group in filteredGroups)
        {
            var regionName =
                group.AssemblyName == "Avalonia" ? "Avalonia" : $"External: {group.AssemblyName}";

            sb.Append(regionIndent).AppendLine($"#region {regionName}");
            sb.AppendLine();

            foreach (var control in group.Controls)
            {
                sb.Append(methodIndent)
                    .Append("/// <summary>Creates a new <c>")
                    .Append(control.FullName.Replace("global::", string.Empty))
                    .AppendLine("</c>.</summary>");
                sb.Append(methodIndent)
                    .Append("protected ")
                    .Append(control.FullName)
                    .Append(' ')
                    .Append(control.MethodName)
                    .Append("() => new ")
                    .Append(control.FullName)
                    .AppendLine("();");
                sb.AppendLine();
            }

            sb.Append(regionIndent).AppendLine("#endregion");
            sb.AppendLine();
        }

        sb.Append(classIndent).AppendLine("}");

        for (var i = containingPath.Count - 1; i >= 0; i--)
        {
            indent--;
            sb.Append(new string(' ', indent * 4)).AppendLine("}");
        }

        if (ns is not null)
            sb.AppendLine("}");

        var hintName =
            $"{(ns is null ? string.Empty : ns + ".")}"
            + $"{(containingPath.Count > 0 ? string.Join(".", containingPath.Select(p => p.Name)) + "." : string.Empty)}"
            + $"{className}.Factories.g.cs";

        spc.AddSource(hintName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static List<ContainingWrapper> BuildContainingTypePath(INamedTypeSymbol view)
    {
        var stack = new Stack<INamedTypeSymbol>();
        for (var t = view.ContainingType; t is not null; t = t.ContainingType)
            stack.Push(t);

        var wrappers = new List<ContainingWrapper>(stack.Count);
        while (stack.Count > 0)
        {
            var t = stack.Pop();
            var tps =
                t.TypeParameters.Length > 0
                    ? "<" + string.Join(", ", t.TypeParameters.Select(p => p.Name)) + ">"
                    : string.Empty;
            var acc = t.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
            wrappers.Add(new ContainingWrapper(acc, t.Name, tps));
        }

        return wrappers;
    }

    private readonly record struct ContainingWrapper(
        string Accessibility,
        string Name,
        string TypeParams
    );

    private readonly record struct ControlInfo(string MethodName, string FullName);

    private sealed record ControlGroup(string AssemblyName, List<ControlInfo> Controls);

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
