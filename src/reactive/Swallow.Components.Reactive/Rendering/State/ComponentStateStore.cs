using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.Rendering.State;

internal sealed class ComponentStateStore : IPersistentComponentStateStore
{
    private readonly Dictionary<string, byte[]> currentState = new();

    public void Initialize(IFormCollection form)
    {
        currentState.Clear();

        foreach (var (key, value) in form.Where(static kvp => kvp.Key.StartsWith(Constants.StatePrefix)))
        {
            var storeKey = key[Constants.StatePrefix.Length..];
            var storeValue = Serialization.DeserializeBinary(value.ToString());

            currentState[storeKey] = storeValue;
        }
    }

    public event EventHandler<IReadOnlyDictionary<string, string>>? OnStatePersisted;

    public IReadOnlyDictionary<string, string> GetSerializedState()
    {
        return currentState.ToDictionary(static kvp => kvp.Key, static kvp => Serialization.SerializeBinary(kvp.Value));
    }

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
            serializedState[key] = Serialization.SerializeBinary(value);
        }

        OnStatePersisted?.Invoke(this, serializedState);
        return Task.CompletedTask;
    }

    public bool SupportsRenderMode(IComponentRenderMode renderMode)
    {
        return renderMode is null or StaticReactiveRenderMode;
    }
}
