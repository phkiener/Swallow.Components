using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Forms;

namespace Swallow.Components.Forms;

/// <summary>
/// A basic text input, similar to <see cref="InputText"/>.
/// </summary>
public partial class SwInputText : InputBase<string?>
{
    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;

        return true;
    }
}
