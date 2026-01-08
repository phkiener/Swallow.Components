using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.Rendering;

/// <summary>
/// Persists an antiforgery token as <c><![CDATA[<meta>]]></c> tags.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class AntiforgeryMetadata(IAntiforgery antiforgery) : ComponentBase
{
    [CascadingParameter]
    public required HttpContext HttpContext { get; set; }

    private AntiforgeryTokenSet? tokenSet;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        tokenSet = HttpContext.Response.HasStarted
            ? antiforgery.GetTokens(HttpContext)
            : antiforgery.GetAndStoreTokens(HttpContext);
    }
}
