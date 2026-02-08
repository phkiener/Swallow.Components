using Microsoft.CodeAnalysis;

namespace IconMappingGenerator;

internal readonly struct GeneratedRoutes(ISymbol type)
{
    public ISymbol Type { get; } = type;

    public static GeneratedRoutes CreateFrom(GeneratorAttributeSyntaxContext context)
    {
        return new GeneratedRoutes(type: context.TargetSymbol);
    }
}
