using Microsoft.JSInterop;

namespace Swallow.Components.Utils;

internal static class InteropHelper
{
    extension(IJSRuntime jsRuntime)
    {
        public async Task<string?> GetFocusedElementIdAsync()
        {
            var focusedElement = await jsRuntime.GetValueAsync<IJSObjectReference>("document.activeElement");
            var id = await focusedElement.GetValueAsync<string?>("id");

            return id;
        }

        public async Task FocusElementWithIdAsync(string id)
        {
            var targetElement = await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementById", [id]);
            await targetElement.InvokeVoidAsync("focus", new { focusVisible = true });
        }
    }
}
