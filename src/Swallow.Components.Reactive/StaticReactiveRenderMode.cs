using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components.Reactive.Framework;

public sealed class StaticReactiveRenderMode : IComponentRenderMode;

public static class Extensions
{
    extension(RenderMode)
    {
        public static StaticReactiveRenderMode StaticReactive => new();
    }
}
