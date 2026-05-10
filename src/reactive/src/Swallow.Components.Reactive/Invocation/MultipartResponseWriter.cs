using System.Buffers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Swallow.Components.Reactive.Invocation;

internal sealed class MultipartContent(string contentType, IHeaderDictionary headers, string content)
{
    public string ContentType { get; } = contentType;
    public IHeaderDictionary Headers { get; } = headers;
    public string Content { get; } = content;

    public static MultipartContent Create(string contentType, string content)
    {
        return new MultipartContent(contentType, new HeaderDictionary(), content);
    }
}

internal sealed class MultipartResponseWriter : IDisposable, IAsyncDisposable
{
    private readonly string boundary = Guid.NewGuid().ToString("N");
    private readonly TextWriter writer;

    public MultipartResponseWriter(HttpResponse response)
    {
        response.ContentType = $"multipart/mixed; boundary={boundary}";

        writer = new HttpResponseStreamWriter(
            stream: response.Body,
            encoding: Encoding.UTF8,
            bufferSize: 16 * 1024,
            bytePool: ArrayPool<byte>.Shared,
            charPool: ArrayPool<char>.Shared);
    }

    public async Task WriteAsync(MultipartContent part)
    {
        await writer.WriteAsync($"--{boundary}\r\n");
        await writer.WriteAsync($"Content-Type: {part.ContentType}\r\n");

        foreach (var header in part.Headers)
        {
            foreach (var value in header.Value)
            {
                await writer.WriteAsync($"{header.Key}: {value}\r\n");
            }
        }

        await writer.WriteAsync("\r\n");
        await writer.WriteAsync(part.Content);
    }

    public async Task FinishAsync()
    {
        await writer.WriteAsync($"\r\n--{boundary}--\r\n");
    }

    public void Dispose()
    {
        writer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await writer.DisposeAsync();
    }
}
