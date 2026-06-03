using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Swallow.Components.Demo.Utils;

public sealed record ComponentParameter(string Name, Type Type, object? DefaultValue, bool Required, bool IsNullable);

public sealed class ComponentParameterInspector(IServiceProvider serviceProvider)
{
    private readonly NullabilityInfoContext nullabilityInfoContext = new();

    public IEnumerable<ComponentParameter> EnumerateParameters(Type componentType)
    {
        var parameterProperties = componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(static p => p.GetCustomAttribute<ParameterAttribute>() is not null)
            .ToList();

        var defaultInstance = ActivatorUtilities.CreateInstance(serviceProvider, componentType);

        foreach (var parameterProperty in parameterProperties)
        {
            var defaultValue = parameterProperty.GetValue(defaultInstance);
            var isRequired = parameterProperty.GetCustomAttribute<EditorRequiredAttribute>() is not null
                || parameterProperty.GetCustomAttribute<RequiredAttribute>() is not null;

            var nullabilityInfo = nullabilityInfoContext.Create(parameterProperty);

            yield return new ComponentParameter(
                Name: parameterProperty.Name,
                Type: parameterProperty.PropertyType,
                DefaultValue: defaultValue,
                Required: isRequired,
                IsNullable: nullabilityInfo.WriteState is NullabilityState.Nullable);
        }
    }
}
