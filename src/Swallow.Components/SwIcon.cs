using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components;

public sealed class SwIcon : ComponentBase
{
    private MarkupString iconMarkup;

    [Parameter]
    [EditorRequired]
    public required IconType Icon { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var manifestPath = IconTypeMapping.ManifestPathFor(Icon);
        await using var fileStream = typeof(IconType).Assembly.GetManifestResourceStream(manifestPath);
        if (fileStream is not null)
        {
            using var reader = new StreamReader(fileStream);
            var markup = await reader.ReadToEndAsync();

            iconMarkup = new MarkupString(markup);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, iconMarkup);
    }
}
