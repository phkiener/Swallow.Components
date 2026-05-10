using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Swallow.Components.Reactive.Test.Fakes;

public sealed class TestHostEnvironment : IWebHostEnvironment
{
    public static TestHostEnvironment Instance { get; } = new();

    public string ApplicationName { get; set; } = "Test Application";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = "./";
    public string EnvironmentName { get; set; } = "Test";
    public string WebRootPath { get; set; } = "./";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}
