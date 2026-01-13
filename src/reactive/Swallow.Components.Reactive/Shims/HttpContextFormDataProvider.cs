using System.Reflection;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Swallow.Components.Reactive.Shims;

internal sealed class HttpContextFormDataProvider
{
    private static readonly Type? underlyingType = typeof(IRazorComponentEndpointInvoker).Assembly.GetType("Microsoft.AspNetCore.Components.Endpoints.HttpContextFormDataProvider");
    private static MethodInfo? setFormDataMethod;

    private readonly object instance;
    private readonly Action<object, string, IReadOnlyDictionary<string, StringValues>, IFormFileCollection> setFormData;

    private HttpContextFormDataProvider(object instance, Action<object, string, IReadOnlyDictionary<string, StringValues>, IFormFileCollection> setFormData)
    {
        this.instance = instance;
        this.setFormData = setFormData;
    }

    public void SetFormData(
        string incomingHandlerName,
        IReadOnlyDictionary<string, StringValues> form,
        IFormFileCollection formFiles)
    {
        setFormData(instance, incomingHandlerName, form, formFiles);
    }

    public static HttpContextFormDataProvider? TryGet(IServiceProvider serviceProvider)
    {
        if (underlyingType is null)
        {
            return null;
        }

        setFormDataMethod ??= underlyingType.GetMethod("SetFormData");
        if (setFormDataMethod is null)
        {
            return null;
        }

        var instance = serviceProvider.GetService(underlyingType);
        if (instance is null)
        {
            return null;
        }

        return new HttpContextFormDataProvider(
            instance: instance,
            setFormData: static (instance, handler, formValues, formFiles) => setFormDataMethod.Invoke(instance, [handler, formValues, formFiles]));
    }
}
