using System.Reflection;
using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Utils;

namespace Swallow.Components.Demo.Layout;

public sealed partial class ComponentExample(RazorRenderer razorRenderer, MarkupRenderer markupRenderer) : ComponentBase
{
    private RenderFragment? renderedComponent;
    private string? renderedRazor;
    private string? renderedMarkup;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public Type? ExampleComponent { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (ChildContent is null && ExampleComponent is null)
        {
            throw new ArgumentException($"At least one of either {nameof(ChildContent)} or {nameof(ExampleComponent)} must be specified.");
        }

        if (ChildContent is not null && ExampleComponent is not null)
        {
            throw new ArgumentException($"You cannot specify both {nameof(ChildContent)} and {nameof(ExampleComponent)}.");
        }

        if (ChildContent is not null)
        {
            renderedComponent = ChildContent;
            renderedRazor = await razorRenderer.RenderAsync(ChildContent);
            renderedMarkup = await markupRenderer.RenderAsync(ChildContent);
        }

        if (ExampleComponent is not null)
        {
            renderedComponent = RenderComponent(ExampleComponent);

            renderedRazor = await LoadEmbeddedFileAsync(ExampleComponent.Assembly, ExampleComponent.FullName + ".razor");
            renderedMarkup = await markupRenderer.RenderAsync(renderedComponent);
        }
    }

    private static async Task<string?> LoadEmbeddedFileAsync(Assembly assembly, string name)
    {
        await using var manifestStream = assembly.GetManifestResourceStream(name);
        if (manifestStream is null)
        {
            return null;
        }

        using var reader = new StreamReader(manifestStream);
        return await reader.ReadToEndAsync();
    }

    private static RenderFragment RenderComponent(Type type)
    {
        return builder =>
        {
            builder.OpenComponent<DynamicComponent>(0);
            builder.AddAttribute(1, nameof(DynamicComponent.Type), type);
            builder.CloseComponent();
        };
    }
}
