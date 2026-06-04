using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Swallow.Components.Features;

namespace Swallow.Components.Inputs;

/// <summary>
/// A simple input for text fields.
/// </summary>
public sealed partial class SwInputText : InputText, ICanDisable, IHasAdditionalAttributes
{
    /// <inheritdoc />
    [Parameter]
    public bool Disabled { get; set; }

    /// <inheritdoc />
    IReadOnlyDictionary<string, object?>? IHasAdditionalAttributes.AdditionalAttributes
    {
        get => AdditionalAttributes!;
        set => AdditionalAttributes = value!;
    }
}
