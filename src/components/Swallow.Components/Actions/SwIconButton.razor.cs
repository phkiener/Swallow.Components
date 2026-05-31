using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Swallow.Components.Features;

namespace Swallow.Components.Actions;

/// <summary>
/// A <see cref="SwButton" /> that displays only an icon.
/// </summary>
public sealed partial class SwIconButton : ComponentBase, ICanDisable, IHandleAttributes
{
    private string buttonType = "";

    protected override void OnParametersSet()
    {
        buttonType = OnClick.HasDelegate ? "button" : "submit";
    }

    /// <summary>
    /// The content to display on the button.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Render this icon button inline with text.
    /// </summary>
    [Parameter]
    public bool Inline { get; set; }

    /// <summary>
    /// A callback invoked when the button is triggered, i.e. clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <inheritdoc />
    [Parameter]
    public bool Disabled { get; set; }

    /// <inheritdoc />
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
