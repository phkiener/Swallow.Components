using System.IO.Compression;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.State;

internal sealed class ComponentStateStore : IPersistentComponentStateStore
{
    private readonly Dictionary<string, byte[]> currentState = new();

    public void Initialize(IFormCollection form)
    {
        currentState.Clear();

        foreach (var (key, value) in form.Where(static kvp => kvp.Key.StartsWith("_srx-state-")))
        {
            var storeKey = key.TrimStart("_srx-state-").ToString();
            var storeValue = DeserializeBrotli(value.ToString());

            currentState[storeKey] = storeValue;
        }
    }

    public event EventHandler<IReadOnlyDictionary<string, string>>? OnStatePersisted;

    public Task<IDictionary<string, byte[]>> GetPersistedStateAsync()
    {
        return Task.FromResult<IDictionary<string, byte[]>>(currentState);
    }

    public Task PersistStateAsync(IReadOnlyDictionary<string, byte[]> state)
    {
        currentState.Clear();

        var serializedState = new Dictionary<string, string>();
        foreach (var (key, value) in state)
        {
            currentState[key] = value;
            serializedState[key] = SerializeBrotli(value);
        }

        OnStatePersisted?.Invoke(this, serializedState);
        return Task.CompletedTask;
    }

    private static string SerializeBrotli(byte[] bytes)
    {
        using var memoryStream = new MemoryStream();
        using var compressingStream = new BrotliStream(memoryStream, CompressionMode.Compress);
        compressingStream.Write(bytes);
        compressingStream.Flush();

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static byte[] DeserializeBrotli(string bytes)
    {
        using var memoryStream = new MemoryStream();
        using var decompressingStream = new BrotliStream(memoryStream, CompressionMode.Decompress);

        var decodedBytes = Convert.FromBase64String(bytes);
        decompressingStream.Write(decodedBytes);
        decompressingStream.Flush();

        return memoryStream.ToArray();
    }
}
