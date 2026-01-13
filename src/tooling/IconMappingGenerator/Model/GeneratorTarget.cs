using Microsoft.CodeAnalysis;

namespace IconMappingGenerator;

internal readonly struct GeneratorTarget(string containingNamespace, string iconTypeClass, string iconTypeMappingClass)
{
    public string ContainingNamespace { get; } = containingNamespace;
    public string IconTypeClass { get; } = iconTypeClass;
    public string IconTypeMappingClass { get; } = iconTypeMappingClass;

    public static GeneratorTarget CreateFrom(GeneratorAttributeSyntaxContext context)
    {
        var constructorArguments = context.Attributes.Single().ConstructorArguments;

        return new GeneratorTarget(
            containingNamespace: context.TargetSymbol.ContainingNamespace.ToDisplayString(),
            iconTypeClass: (string)constructorArguments[0].Value!,
            iconTypeMappingClass: context.TargetSymbol.Name);
    }
}
