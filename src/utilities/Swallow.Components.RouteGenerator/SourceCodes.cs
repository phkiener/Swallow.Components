using System.CodeDom.Compiler;

namespace Swallow.Components.RouteGenerator;

internal static class SourceCodes
{
    public const string MarkerAttribute = """
        namespace Swallow.Components.RouteGenerator
        {
            [global::System.CodeDom.Compiler.GeneratedCode("IconMappingGenerator", "1.0.0.0")]
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class GenerateComponentRouteAttribute : global::System.Attribute;
        }
        """;

    public const string OutputMarkerAttribute = """
        namespace Swallow.Components.RouteGenerator
        {
            [global::System.CodeDom.Compiler.GeneratedCode("IconMappingGenerator", "1.0.0.0")]
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class GeneratedRoutesAttribute : global::System.Attribute;
        }
        """;

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
