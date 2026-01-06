using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive;

public static class PersistentComponentStateExtensions
{
    extension(PersistentComponentState state)
    {
        public IDisposable Register<T>(string name, ref T value, Func<T> callback)
        {
            return state.Register(name, ref value, callback, null);
        }

        public IDisposable Register<T>(string name, ref T value, Func<T> callback, IComponentRenderMode? renderMode)
        {
            if (state.TryTakeFromJson<T>(name, out var result) && result != null)
            {
                value = result;
            }

            return state.RegisterOnPersisting(Persist, renderMode);

            Task Persist()
            {
                state.PersistAsJson(name, callback.Invoke());
                return Task.CompletedTask;
            }
        }
    }
}
