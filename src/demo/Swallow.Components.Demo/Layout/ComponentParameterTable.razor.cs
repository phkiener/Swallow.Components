using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Utils;
using Swallow.Components.Features;

namespace Swallow.Components.Demo.Layout;

public sealed partial class ComponentParameterTable<TComponent>(ComponentParameterInspector inspector) : ComponentBase where TComponent : ComponentBase
{
    private List<ComponentParameter> parameters = [];

    protected override void OnInitialized()
    {
        parameters = inspector.EnumerateParameters(typeof(TComponent))
            .OrderBy(static p => p.Name is nameof(IHasAdditionalAttributes.AdditionalAttributes) ? $"zzz{p.Name}" : p.Name)
            .ToList();
    }
}
