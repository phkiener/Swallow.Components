using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Swallow.Components.Reactive.Rendering;

/// <summary>
/// Persists an antiforgery token as <c><![CDATA[<meta>]]></c> tags.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class AntiforgeryMetadata(IServiceProvider serviceProvider) : ComponentBase
{
    private AntiforgeryRequestToken? antiforgeryToken;

    protected override void OnInitialized()
    {
        antiforgeryToken = serviceProvider.GetService<AntiforgeryStateProvider>()?.GetAntiforgeryToken();
    }
}
