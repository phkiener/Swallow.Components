using System.Buffers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Swallow.Components.Reactive.Invocation;

internal sealed class MultipartContent(IHeaderDictionary headers, string content)
{
    public IHeaderDictionary Headers { get; } = headers;
    public string Content { get; } = content;

    public static MultipartContent Create(string contentType, string content)
    {
        var headerDictionary = new HeaderDictionary();
        headerDictionary.Append("Content-Type", contentType);

        return new MultipartContent(headerDictionary, content);
    }
}

internal sealed class MultipartResponseWriter : IDisposable, IAsyncDisposable
{
    private readonly string boundary = Guid.NewGuid().ToString("N");
    private readonly TextWriter writer;
    private bool isClosed = false;

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
        if (isClosed)
        {
            throw new InvalidOperationException("The multipart response is already closed.");
        }

        await writer.WriteAsync($"--{boundary}\r\n");
        foreach (var header in part.Headers)
        {
            foreach (var value in header.Value)
            {
                await writer.WriteAsync($"{header.Key}: {value}\r\n");
            }
        }

        await writer.WriteAsync("\r\n");
        await writer.WriteAsync(part.Content);
        await writer.WriteAsync("\r\n");
    }

    public void Dispose()
    {
        if (isClosed)
        {
            return;
        }

        writer.Write($"\r\n--{boundary}--\r\n");
        writer.Dispose();

        isClosed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (isClosed)
        {
            return;
        }

        await writer.WriteAsync($"\r\n--{boundary}--\r\n");
        await writer.DisposeAsync();

        isClosed = true;
    }
}
