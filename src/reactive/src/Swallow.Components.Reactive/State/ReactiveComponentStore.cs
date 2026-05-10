using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.State;

internal sealed class ReactiveComponentStore : IPersistentComponentStateStore
{
    private Dictionary<string, byte[]> currentState = new();

    public void Initialize(IFormCollection form)
    {
        foreach (var key in form.Keys)
        {
            var value = form[key].LastOrDefault();
            if (value is null)
            {
                continue;
            }

            currentState[key] = Convert.FromBase64String(value);
        }
    }

    public Dictionary<string, string> Serialize()
    {
        return currentState.ToDictionary(static kvp => kvp.Key, static kvp => Convert.ToBase64String(kvp.Value));
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
}
