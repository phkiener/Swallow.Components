using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Hosting;

public abstract partial class AppRoutes : ComponentBase
{
    protected abstract Assembly HostAssembly { get; }

    public static Assembly[] AdditionalAssemblies { get; } = [typeof(AppRoutes).Assembly];
    public static Type NotFoundPage { get; } = typeof(NotFoundPage);
}
