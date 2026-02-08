using System.CodeDom.Compiler;
using System.Collections.Immutable;
using IconMappingGenerator;
using Microsoft.CodeAnalysis;

namespace Swallow.Components.RouteGenerator;

[Generator]
public sealed class ComponentRouteGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static c => c.AddSource("GenerateComponentRouteAttribute.g.cs", SourceCodes.MarkerAttribute));
        context.RegisterPostInitializationOutput(static c => c.AddSource("GeneratedRoutesAttribute.g.cs", SourceCodes.OutputMarkerAttribute));

        var components = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Swallow.Components.RouteGenerator.GenerateComponentRouteAttribute",
            static (_, _) => true,
            static (x, _) => MarkedComponent.CreateFrom(x));

        var outputs = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Swallow.Components.RouteGenerator.GeneratedRoutesAttribute",
            static (_, _) => true,
            static (x, _) => GeneratedRoutes.CreateFrom(x));

        context.RegisterSourceOutput(components.Collect().Combine(outputs.Collect()), GenerateComponentRoutes);
    }

    private static void GenerateComponentRoutes(SourceProductionContext context, (ImmutableArray<MarkedComponent> Components, ImmutableArray<GeneratedRoutes> Outputs) parameters)
    {
        foreach (var output in parameters.Outputs)
        {
            using var content = new StringWriter();
            using var writer = new IndentedTextWriter(content);

            foreach (var group in parameters.Components.GroupBy(static c => c.Component.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                writer.WriteLine($"// {group.Key}");
                foreach (var component in group)
                {
                    writer.WriteLine($"// {component.Component.Name}");
                }

                writer.WriteLine();
            }

            context.AddSource($"{output.Type.ContainingNamespace}.{output.Type.Name}.g.cs", content.ToString());
        }
    }
}
