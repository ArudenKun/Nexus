using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Nexus.Avalonia.SourceGenerator;

[Generator]
public class StaticResourcesGenerator : IIncrementalGenerator
{
    private const string AttributeMetadataName =
        "Avalonia.Markup.Declarative.GenerateStaticResourcesAttribute";

    private const string AttributeMetadataContent = """
        namespace Avalonia.Markup.Declarative
        {
            using System;

            /// <summary>
            /// Overrides the root name of the static resource class emitted by
            /// <c>StaticResourcesGenerator</c>. Defaults to <c>StaticResources</c>.
            /// </summary>
            [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
            public sealed class GenerateStaticResourcesAttribute : Attribute
            {
                public GenerateStaticResourcesAttribute(string rootClassName)
                {
                    RootClassName = rootClassName;
                }

                public string RootClassName { get; }
            }
        }
        """;

    private const string DefaultRootClassName = "StaticResources";
    private const string AvaloniaResourceMetadata = "AvaloniaResource";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(postContext =>
        {
            postContext.AddSource($"{AttributeMetadataName}.g.cs", AttributeMetadataContent);
        });

        // ── Root class name from the assembly attribute (if any) ─────────────
        var rootNameProvider = context.CompilationProvider.Select(
            (compilation, _) =>
            {
                var attributeSymbol = compilation.GetTypeByMetadataName(AttributeMetadataName);
                if (attributeSymbol is null)
                    return DefaultRootClassName;

                foreach (var attr in compilation.Assembly.GetAttributes())
                {
                    if (
                        !SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol)
                    )
                        continue;

                    if (attr.ConstructorArguments.Length < 1)
                        continue;

                    if (
                        attr.ConstructorArguments[0].Value is string name
                        && !string.IsNullOrWhiteSpace(name)
                    )
                        return name;
                }

                return DefaultRootClassName;
            }
        );

        // ── AvaloniaResource files only (via AdditionalFiles metadata) ──────
        var assetsProvider = context
            .AdditionalTextsProvider.Select(
                (text, ct) =>
                {
                    var options = text.GetText(ct)?.Container;
                    return text;
                }
            )
            .Where(text =>
            {
                // The AvaloniaResource metadata is inspected later via
                // CompilationProvider because AdditionalTextsProvider by itself
                // doesn't expose item metadata; the MSBuild plumbing aliases
                // them under AdditionalFiles so we filter by path prefix here
                // and re-verify against the project item list in the combine
                // step below.
                var path = text.Path.Replace('\\', '/');
                return path.Contains("/Assets/") || path.StartsWith("Assets/");
            })
            .Collect();

        // Also read the project's declared AvaloniaResource identities so we
        // can filter out AdditionalFiles that were not from AvaloniaResource.
        var avaloniaResourceNamesProvider = context
            .AdditionalTextsProvider.Select((text, _) => text.Path.Replace('\\', '/'))
            .Collect();

        var combined = assetsProvider
            .Combine(rootNameProvider)
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, (spc, tuple) =>
        {
            var ((assets, rootName), compilation) = tuple;

            if (assets.IsDefaultOrEmpty)
                return;

            // Build the set of resource paths that came from AvaloniaResource items.
            // MSBuild aliases them under AdditionalFiles with the path relative to
            // the project root, so we normalise and filter.
            var assetsInfos = ImmutableArray.CreateBuilder<AssetInfo>();

            foreach (var text in assets)
            {
                var normalized = text.Path.Replace('\\', '/');

                // Only accept files under Assets/ — matches the default
                //   <AvaloniaResource Include="Assets\**" />
                // If you use a different root, change this prefix.
                if (!normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    continue;

                // AdditionalFiles sometimes arrives with an absolute path when
                // the project is built from a directory with spaces or through
                // an SDK that normalises differently. Trim to the Assets/ root.
                var idx = normalized.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    continue;
                normalized = normalized.Substring(idx);

                var segments = normalized.Split('/');
                if (segments.Length < 2)
                    continue;

                var folderPath = segments.Take(segments.Length - 1).ToList();
                var rawName = Path.GetFileNameWithoutExtension(segments[^1]);
                var extension = Path.GetExtension(segments[^1]);

                var constantName = PascalCase(rawName + extension.Replace(".", string.Empty));
                var uriPath = "/" + normalized;

                assetsInfos.Add(new AssetInfo(folderPath, constantName, uriPath));
            }

            if (assetsInfos.Count == 0)
                return;

            Emit(spc, assetsInfos.ToImmutable(), rootName);
        });
    }

    private static void Emit(
        SourceProductionContext spc,
        ImmutableArray<AssetInfo> assets,
        string rootClassName
    )
    {
        var ns = "Avalonia.Markup.Declarative";

        var root = new FolderNode(string.Empty);

        foreach (var asset in assets)
        {
            var cursor = root;
            foreach (var segment in asset.FolderPath)
            {
                if (!cursor.Children.TryGetValue(segment, out var child))
                {
                    child = new FolderNode(segment);
                    cursor.Children[segment] = child;
                }
                cursor = child;
            }
            cursor.Assets.Add(asset);
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.Append("namespace ").AppendLine(ns);
        sb.AppendLine("{");
        sb.Append("    public static partial class ").Append(rootClassName).AppendLine();
        sb.AppendLine("    {");

        EmitFolder(sb, root, indent: 2, defaultAccessibility: "public");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        spc.AddSource($"{ns}.{rootClassName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void EmitFolder(
        StringBuilder sb,
        FolderNode node,
        int indent,
        string defaultAccessibility
    )
    {
        var pad = new string(' ', indent * 4);

        foreach (var child in node.Children.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            var className = PascalCase(child.Key);

            sb.Append(pad)
                .Append(defaultAccessibility)
                .Append(" static class ")
                .Append(className)
                .AppendLine();
            sb.Append(pad).AppendLine("{");

            EmitFolder(sb, child.Value, indent + 1, defaultAccessibility);

            foreach (
                var asset in child.Value.Assets.OrderBy(a => a.ConstantName, StringComparer.Ordinal)
            )
            {
                var methodPad = new string(' ', (indent + 1) * 4);
                sb.Append(methodPad)
                    .Append("public const string ")
                    .Append(asset.ConstantName)
                    .Append(" = \"")
                    .Append(asset.UriPath)
                    .AppendLine("\";");
            }

            sb.Append(pad).AppendLine("}");
            sb.AppendLine();
        }

        foreach (var asset in node.Assets.OrderBy(a => a.ConstantName, StringComparer.Ordinal))
        {
            sb.Append(pad)
                .Append("public const string ")
                .Append(asset.ConstantName)
                .Append(" = \"")
                .Append(asset.UriPath)
                .AppendLine("\";");
        }
    }

    private static string PascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder(name.Length);
        var upperNext = true;

        foreach (var ch in name)
        {
            if (!char.IsLetterOrDigit(ch))
            {
                upperNext = true;
                continue;
            }

            sb.Append(upperNext ? char.ToUpperInvariant(ch) : ch);
            upperNext = false;
        }

        return sb.ToString();
    }

    private readonly record struct AssetInfo(
        List<string> FolderPath,
        string ConstantName,
        string UriPath
    );

    private sealed class FolderNode
    {
        public FolderNode(string name) => Name = name;

        public string Name { get; }
        public Dictionary<string, FolderNode> Children { get; } = new(StringComparer.Ordinal);
        public List<AssetInfo> Assets { get; } = new();
    }
}
