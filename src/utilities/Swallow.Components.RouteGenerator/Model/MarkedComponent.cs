using Microsoft.CodeAnalysis;

namespace IconMappingGenerator;

internal readonly struct MarkedComponent(ISymbol component, string routeTemplate)
{
    public ISymbol Component { get; } = component;
    public string RouteTemplate { get; } = routeTemplate;

    public static MarkedComponent CreateFrom(GeneratorAttributeSyntaxContext context)
    {
        var routeAttribute = context.TargetSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() is "Microsoft.AspNetCore.Components.RouteAttribute");
        return new MarkedComponent(
            component: context.TargetSymbol,
            routeTemplate: routeAttribute?.ConstructorArguments[0].Value?.ToString() ?? "");
    }
}
