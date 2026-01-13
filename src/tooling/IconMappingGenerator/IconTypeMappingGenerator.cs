using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace IconMappingGenerator;

[Generator]
public sealed class IconTypeMappingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static c => c.AddSource("IconTypeMappingAttribute.g.cs", SourceCodes.MarkerAttribute));

        var files = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Where(static c => IncludedIcon.IsRelevant(c.Left, c.Right))
            .Select(static (c, _) => IncludedIcon.CreateFrom(c.Left, c.Right));

        var iconMapping = context.SyntaxProvider.ForAttributeWithMetadataName(
            "IconMappingGenerator.GenerateIconTypeMappingAttribute",
            static (_, _) => true,
            static (x, _) => GeneratorTarget.CreateFrom(x));

        context.RegisterSourceOutput(iconMapping.Combine(files.Collect()), GenerateIconMapping);
    }

    private static void GenerateIconMapping(SourceProductionContext context, (GeneratorTarget Container, ImmutableArray<IncludedIcon> EmbeddedIcons) data)
    {
        var sortedIcons = data.EmbeddedIcons.OrderBy(static i => i.CodeName).ToList();

        var iconType = SourceCodes.IconType(
            containingNamespace: data.Container.ContainingNamespace,
            name: data.Container.IconTypeClass,
            embeddedIcons: sortedIcons);

        var mapping = SourceCodes.IconTypeMapping(
            containingNamespace: data.Container.ContainingNamespace,
            name: data.Container.IconTypeMappingClass,
            iconTypeName: data.Container.IconTypeClass,
            embeddedIcons: sortedIcons);

        context.AddSource($"{data.Container.ContainingNamespace}.{data.Container.IconTypeClass}.g.cs", iconType);
        context.AddSource($"{data.Container.ContainingNamespace}.{data.Container.IconTypeMappingClass}.g.cs", mapping);
    }
}
