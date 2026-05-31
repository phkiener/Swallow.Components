using System.Reflection;
using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Layout;
using Swallow.Components.Demo.Pages;

namespace Swallow.Components.Demo;

public abstract partial class AppRoutes : ComponentBase
{
    protected abstract Assembly HostAssembly { get; }
    protected virtual Type MainLayout { get; } = typeof(MainLayout);

    public static Assembly[] AdditionalAssemblies { get; } = [typeof(AppRoutes).Assembly];
    public static Type NotFoundPage { get; } = typeof(NotFoundPage);
}
