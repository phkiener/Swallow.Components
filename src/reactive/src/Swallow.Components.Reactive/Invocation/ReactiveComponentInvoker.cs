using System.Net.Mime;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Swallow.Components.Reactive.State;

namespace Swallow.Components.Reactive.Invocation;

internal sealed class ReactiveComponentInvoker(ReactiveComponentRenderer renderer)
{
    public async Task InvokeAsync(Type componentType, HttpContext context)
    {
        if (!IsValidRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var form = await context.Request.ReadFormAsync();

        var store = new ReactiveComponentStore();
        store.Initialize(form);

        await renderer.InitializeComponentServicesAsync(context, store);

        // Exceptions while invoking should be propagated to the default exception handler.
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append(ReactiveHeaders.ResponseMarker, "true");
        await using var writer = new MultipartResponseWriter(context.Response);

        try
        {
            // TODO: Parameters
            await renderer.RehydrateComponent(componentType);
            await renderer.WaitForSettled();

            // TODO: Streaming.
            // Step 2: Invoke event handler.

            await renderer.WaitForSettled();

            var markupPart = await CreateMarkupPartAsync(renderer);
            await writer.WriteAsync(markupPart);

            var statePart = CreateStatePart(store);
            await writer.WriteAsync(statePart);

            // TODO: Discover & write event handlers

            await writer.FinishAsync();
        }
        catch (NavigationException navigation)
        {
            var content = MultipartContent.Create(MediaTypeNames.Text.Plain, navigation.Location);
            content.Headers.Append(ReactiveHeaders.RedirectMarker, "true");

            await writer.WriteAsync(content);
            await writer.FinishAsync();
        }
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

        if (request.Headers.Referer.Count is 0)
        {
            return false;
        }

        if (!request.HasFormContentType)
        {
            return false;
        }

        return true;
    }

    private static MultipartContent CreateStatePart(ReactiveComponentStore store)
    {
        var serializedState = store.Serialize();
        var content = string.Join('&', serializedState.Select(static kvp => $"{kvp.Key}={kvp.Value}"));

        var statePart = MultipartContent.Create(MediaTypeNames.Application.FormUrlEncoded, content);
        statePart.Headers.Append(ReactiveHeaders.ResponsePartIdentifier, ReactiveHeaders.ResponsePartState);

        return statePart;
    }

    private static async Task<MultipartContent> CreateMarkupPartAsync(ReactiveComponentRenderer renderer)
    {
        var markup = await renderer.RenderMarkupAsync();
        return MultipartContent.Create(MediaTypeNames.Text.Html, markup);
    }
}
