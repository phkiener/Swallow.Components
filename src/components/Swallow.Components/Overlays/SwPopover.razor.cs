using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Overlays;

public sealed partial class SwPopover : ComponentBase
{
    private readonly string anchorName = $"sw-{Guid.NewGuid():N}";
    private string? popoverClass;
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
    /// Callback that is invoked when the tooltip is being shown.
    /// </summary>
    [Parameter]
    public EventCallback OnTooltipShown { get; set; }

    /// <summary>
    /// Callback that is invoked when the tooltip is no longer being shown.
    /// </summary>
    [Parameter]
    public EventCallback OnTooltipHidden { get; set; }

    protected override void OnParametersSet()
    {
        triggerClass = DisplayMode switch
        {
            DisplayMode.Inline => "inline",
            DisplayMode.Block => "block",
            _ => throw new ArgumentOutOfRangeException(nameof(DisplayMode))
        };

        popoverClass = TooltipPosition switch
        {
            Position.Above => "above",
            Position.Below => "below",
            Position.Left => "left",
            Position.Right => "right",
            _ => throw new ArgumentOutOfRangeException(nameof(TooltipPosition))
        };
    }
}
