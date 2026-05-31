namespace Swallow.Components.Actions;

/// <summary>
/// The variant of a button.
/// </summary>
public enum ButtonVariant
{
    /// <summary>
    /// A highlighted button, drawing attention.
    /// </summary>
    Primary = 1,

    /// <summary>
    /// The default button.
    /// </summary>
    Default = 2,

    /// <summary>
    /// A less pronounced button, shown only as text without decoration.
    /// </summary>
    Text = 3,
}

/// <summary>
/// Extensions for <see cref="ButtonVariant"/>.
/// </summary>
public static class ButtonVariantExtensions
{
    extension(ButtonVariant variant)
    {
        /// <summary>
        /// The expected class name for this variant, if any.
        /// </summary>
        public string? ClassName => variant switch
        {
            ButtonVariant.Primary => "primary",
            ButtonVariant.Text => "text",
            _ => null
        };
    }
}
