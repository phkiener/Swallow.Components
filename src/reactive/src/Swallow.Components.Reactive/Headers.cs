namespace Swallow.Components.Reactive;

internal static class Headers
{
    public const string RequestMarker = "srx-request";
    public const string StreamingRequestMarker = "srx-streaming";

    public const string ResponseMarker = "srx-response";
    public const string RedirectMarker = "srx-redirect";

    public const string ResponsePartIdentifier = "srx-kind";
    public const string ResponsePartState = "state";
}
