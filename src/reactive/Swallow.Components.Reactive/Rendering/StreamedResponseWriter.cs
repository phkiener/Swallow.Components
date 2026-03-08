using System.Buffers;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Swallow.Components.Reactive.Rendering;

internal sealed class StreamedResponseWriter(Stream stream, Encoding encoding, int bufferSize, ArrayPool<byte> bytePool, ArrayPool<char> charPool)
    : HttpResponseStreamWriter(stream, encoding, bufferSize, bytePool, charPool)
{
    public string Boundary { get; } = $"<!-- SRX-STREAM-BOUNDARY {Guid.NewGuid():N} -->";

    public override void Flush()
    {
        base.WriteLine(Boundary);
        base.Flush();
    }

    public override Task FlushAsync()
    {
        base.WriteLine(Boundary);
        return base.FlushAsync();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        base.WriteLine(Boundary);
        return base.FlushAsync(cancellationToken);
    }
}
