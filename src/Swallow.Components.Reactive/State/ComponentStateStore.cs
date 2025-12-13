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
            var storeValue = Convert.FromBase64String(value.ToString());

            currentState[storeKey] = storeValue;
        }
    }

    public event EventHandler? OnStatePersisted;
    public IReadOnlyDictionary<string, byte[]> CurrentState => currentState;

    public Task<IDictionary<string, byte[]>> GetPersistedStateAsync()
    {
        return Task.FromResult<IDictionary<string, byte[]>>(currentState);
    }

    public Task PersistStateAsync(IReadOnlyDictionary<string, byte[]> state)
    {
        currentState.Clear();
        foreach (var (key, value) in state)
        {
            currentState[key] = value;
        }

        OnStatePersisted?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
