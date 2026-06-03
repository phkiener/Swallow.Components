using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Swallow.Components.Features;

namespace Swallow.Components.Actions;

/// <summary>
/// A simple button.
/// </summary>
/// <remarks>
/// While a <c>type</c> is inferred based on whether <see cref="OnClick"/> is set or not, it can
/// still be overridden by setting <c>type="..."</c> explicitly.
/// </remarks>
public sealed partial class SwButton : ComponentBase, ICanDisable, IHasAdditionalAttributes
{
    private string buttonType = "";

    /// <inheritdoc />
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
    /// The <see cref="ButtonVariant"/> for this button.
    /// </summary>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Default;

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
