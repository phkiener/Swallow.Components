using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.Routing;

internal sealed class ReactiveComponentInvoker
{
    public async Task InvokeAsync(Type componentType, HttpContext context)
    {
        if (!IsValidRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // Exceptions while invoking should be propagated to the default exception handler.
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append(ReactiveHeaders.ResponseMarker, "true");

        // Step 1: Rehydrate from state.
        // Step 2: Invoke event handler.
        // Step 3: Render current DOM
        // Step 4: Serialize state
        // Step 5: Write multipart response

        // TODO: Streaming.
    }

    private static bool IsValidRequest(HttpRequest request)
    {
        if (request.Method != HttpMethods.Post)
        {
            return false;
        }

        if (request.Headers.ContainsKey(ReactiveHeaders.RequestMarker))
        {
            return false;
        }

        return true;
    }
}
