using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Overlays;

/// <summary>
/// A tooltip that reveals the <see cref="TooltipContent"/> when hovering
/// over <see cref="TriggerContent"/>.
/// </summary>
public sealed partial class SwTooltip : ComponentBase
{
    private readonly string anchorName = $"sw-{Guid.NewGuid():N}";
    private string? tooltipClass;
    private string? triggerClass;

    /// <summary>
    /// The content that triggers the tooltip.
    /// </summary>
    /// <remarks>
    /// The content is wrapped inside another element, depending on the
    /// <see cref="DisplayMode"/>.
    /// </remarks>
    [Parameter]
    [EditorRequired]
    public required RenderFragment TriggerContent { get; set; }

    /// <summary>
    /// The content to show when hovering.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment TooltipContent { get; set; }

    /// <summary>
    /// Whether to wrap <see cref="TriggerContent"/> inside a
    /// <c><![CDATA[<span>]]></c> or a <c><![CDATA[<div>]]></c>.
    /// </summary>
    [Parameter]
    public DisplayMode DisplayMode { get; set; } = DisplayMode.Inline;

    /// <summary>
    /// Where to position the <see cref="TooltipContent"/> relative to
    /// <see cref="TriggerContent"/>.
    /// </summary>
    [Parameter]
    public Position TooltipPosition { get; set; } = Position.Below;

    /// <summary>
    /// Callback that is invoked when the popover is <em>toggled</em>, i.e.
    /// both when it is being shown and being hidden.
    /// </summary>
    /// <remarks>
    /// It is both triggered when the popover is shown, <em>and</em> when
    /// it is hidden. This is due to the fact that, fundamentally, we're
    /// relying on the <c>toggle</c> event on the element. Blazor configured
    /// this event to a plain <see cref="EventArgs"/> instead of an e.g.
    /// <c>ToggleEventArgs</c> containing the new state - so we simply
    /// don't know whether the popover was opened or closed.
    /// </remarks>
    [Parameter]
    public EventCallback OnTriggered { get; set; }

    protected override void OnParametersSet()
    {
        triggerClass = DisplayMode switch
        {
            DisplayMode.Inline => "inline",
            DisplayMode.Block => "block",
            _ => throw new ArgumentOutOfRangeException(nameof(DisplayMode))
        };

        tooltipClass = TooltipPosition switch
        {
            Position.Above => "above",
            Position.Below => "below",
            Position.Left => "left",
            Position.Right => "right",
            _ => throw new ArgumentOutOfRangeException(nameof(TooltipPosition))
        };
    }
}
