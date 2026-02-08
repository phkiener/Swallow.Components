using System.CodeDom.Compiler;
using System.Text;
using IconMappingGenerator;
using Microsoft.CodeAnalysis.Text;

namespace Swallow.Components.RouteGenerator;

internal static class SourceCodes
{
    public const string MarkerAttribute = """
        namespace Swallow.Components.RouteGenerator
        {
            [global::System.CodeDom.Compiler.GeneratedCode("Swallow.Components.RouteGenerator", "1.0.0.0")]
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class GenerateComponentRouteAttribute : global::System.Attribute;
        }
        """;

    public const string OutputMarkerAttribute = """
        namespace Swallow.Components.RouteGenerator
        {
            [global::System.CodeDom.Compiler.GeneratedCode("Swallow.Components.RouteGenerator", "1.0.0.0")]
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class GeneratedRoutesAttribute : global::System.Attribute;
        }
        """;

    public static SourceText IconTypeMapping(
        string containingNamespace,
        string name,
        IReadOnlyList<MarkedComponent> components)
    {
        using var sourceCode = new StringWriter();
        var writer = new IndentedTextWriter(sourceCode);

        writer.WriteLine($"namespace {containingNamespace}");
        using (writer.BeginBlock())
        {
            writer.WriteLine(@"[global::System.CodeDom.Compiler.GeneratedCode(""Swallow.Components.RouteGenerator"", ""1.0.0.0"")]");
            writer.WriteLine($"partial class {name}");
            using (writer.BeginBlock())
            {
                foreach (var component in components)
                {
                    writer.WriteLine("/// <summary>");
                    writer.WriteLine($"/// A route that will lead to <see cref=\"{component.Component.ContainingNamespace.ToDisplayString()}.{component.Component.Name}\" />");
                    writer.WriteLine("/// </summary>");
                    writer.WriteLine($"public static string {component.Component.Name} => \"{component.RouteTemplate}\";");
                    writer.WriteLine();
                }
            }
        }

        writer.Flush();
        return SourceText.From(sourceCode.ToString(), Encoding.UTF8);
    }

    private static IDisposable BeginBlock(this IndentedTextWriter writer)
    {
        writer.WriteLine("{");
        writer.Indent += 1;

        return new CloseBlock(writer);
    }

    private sealed class CloseBlock(IndentedTextWriter writer) : IDisposable
    {
        public void Dispose()
        {
            writer.Indent -= 1;
            writer.WriteLine("}");
        }
    }
}
