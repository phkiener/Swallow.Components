using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Swallow.Components.Reactive.Rendering;

internal sealed class Serialization
{
    private static readonly JsonSerializerOptions ValueSerializerOptions = new()
    {
        MaxDepth = 32,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static byte[] DeserializeBinary(string serialized)
    {
        var decodedBytes = Convert.FromBase64String(serialized);
        using var memoryStream = new MemoryStream(decodedBytes);
        using var decompressingStream = new BrotliStream(memoryStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        decompressingStream.CopyTo(outputStream);
        decompressingStream.Flush();

        return outputStream.ToArray();
    }

    public static string SerializeBinary(byte[] data)
    {
        using var memoryStream = new MemoryStream();
        using var compressingStream = new BrotliStream(memoryStream, CompressionMode.Compress);
        compressingStream.Write(data);
        compressingStream.Flush();

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static object? DeserializeJson(string serialized, Type type)
    {
        var bytes = DeserializeBinary(serialized);

        return JsonSerializer.Deserialize(Encoding.UTF8.GetString(bytes), type, ValueSerializerOptions);
    }

    public static string SerializeJson<T>(T? data)
    {
        var json = JsonSerializer.Serialize(data, ValueSerializerOptions);

        return SerializeBinary(Encoding.UTF8.GetBytes(json));
    }
}
