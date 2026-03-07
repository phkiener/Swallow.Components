namespace Swallow.Components.Reactive.Rendering;

internal sealed class StreamedResponseWriter(TextWriter writer)
{
    public string Boundary { get; } = $"<!-- SRX-STREAM-BOUNDARY {Guid.NewGuid():N} -->";
    public TextWriter Writer { get; } = writer;
}
