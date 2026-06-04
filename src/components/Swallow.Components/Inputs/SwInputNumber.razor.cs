using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Swallow.Components.Features;

namespace Swallow.Components.Inputs;

/// <summary>
/// A simple input for numbers.
/// </summary>
/// <typeparam name="T">The type of number to bind to.</typeparam>
/// <remarks>
/// Juste like for <see cref="InputNumber{TValue}"/>, the supported types for <typeparamref name="T"/> are <see cref="int"/>,
/// <see cref="long"/>, <see cref="short"/>, <see cref="float"/>, <see cref="double"/> and <see cref="decimal"/>.
/// </remarks>
public sealed partial class SwInputNumber<T> : InputNumber<T>, ICanDisable, IHasAdditionalAttributes
{
    private static string DetermineInputMode()
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (targetType == typeof(float)
            || targetType == typeof(double)
            || targetType == typeof(decimal))
        {
            return "decimal";
        }

        return "numeric";
    }

    private readonly string InputMode = DetermineInputMode();

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
