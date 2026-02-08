using Microsoft.CodeAnalysis;

namespace IconMappingGenerator;

internal readonly struct MarkedComponent(ISymbol component, string routeTemplate)
{
    public ISymbol Component { get; } = component;
    public string RouteTemplate { get; } = routeTemplate;

    public static MarkedComponent CreateFrom(GeneratorAttributeSyntaxContext context)
    {
        return new MarkedComponent(
            component: context.TargetSymbol,
            routeTemplate: "");
    }
}
