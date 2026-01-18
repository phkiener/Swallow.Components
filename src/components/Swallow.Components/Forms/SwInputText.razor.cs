using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Swallow.Components.Forms;

/// <summary>
/// A text input supporting a handful of decorations.
/// </summary>
public partial class SwInputText : InputBase<string?>
{
    /// <summary>
    /// A prefix to render before the actual input.
    /// </summary>
    [Parameter]
    public RenderFragment? Prefix { get; set; }

    /// <summary>
    /// A suffx to render after the actual input.
    /// </summary>
    [Parameter]
    public RenderFragment? Suffix { get; set; }

    /// <summary>
    /// An action, most likely a button, to render after the suffix.
    /// </summary>
    [Parameter]
    public RenderFragment? Action { get; set; }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;

        return true;
    }
}
