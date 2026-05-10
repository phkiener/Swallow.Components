using System.IO.Compression;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.State;

internal sealed class ReactiveComponentStore(IDataProtectionProvider dataProtectionProvider) : IPersistentComponentStateStore
{
    private const string Purpose = "ReactiveComponent-State";

    private Dictionary<string, byte[]> currentState = new();

    public void Initialize(IFormCollection form)
    {
        foreach (var key in form.Keys)
        {
            if (!key.StartsWith(Constants.StatePrefix))
            {
                continue;
            }

            var value = form[key].LastOrDefault();
            if (value is null)
            {
                continue;
            }

            var stateKey = key[Constants.StatePrefix.Length..];
            currentState[stateKey] = Deserialize(value);
        }
    }

    public Dictionary<string, string> Serialize()
    {
        return currentState.ToDictionary(
            static kvp => Constants.StatePrefix + kvp.Key,
            kvp => Serialize(kvp.Value));
    }

    Task<IDictionary<string, byte[]>> IPersistentComponentStateStore.GetPersistedStateAsync()
    {
        return Task.FromResult<IDictionary<string, byte[]>>(currentState);
    }

    Task IPersistentComponentStateStore.PersistStateAsync(IReadOnlyDictionary<string, byte[]> state)
    {
        currentState = new Dictionary<string, byte[]>(state);

        return Task.CompletedTask;
    }

    private byte[] Deserialize(string formContent)
    {
        var protectedBytes = Convert.FromBase64String(formContent);
        var protector = dataProtectionProvider.CreateProtector(Purpose);
        var decodedBytes = protector.Unprotect(protectedBytes);

        using var memoryStream = new MemoryStream(decodedBytes);
        using var decompressingStream = new BrotliStream(memoryStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        decompressingStream.CopyTo(outputStream);
        decompressingStream.Flush();

        return outputStream.ToArray();
    }

    private string Serialize(byte[] state)
    {
        using var memoryStream = new MemoryStream();
        using var compressingStream = new BrotliStream(memoryStream, CompressionMode.Compress);
        compressingStream.Write(state);
        compressingStream.Flush();

        var protector = dataProtectionProvider.CreateProtector(Purpose);
        var protectedData = protector.Protect(memoryStream.ToArray());

        return Convert.ToBase64String(protectedData);
    }
}
