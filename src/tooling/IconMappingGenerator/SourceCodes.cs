using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace IconMappingGenerator;

internal static class SourceCodes
{
    public const string IncludeIconMetadata = "build_metadata.AdditionalFiles.IconMappingGenerator";
    public const string ResourceNameMetadata = "build_metadata.AdditionalFiles.IconMappingGenerator_Resource";
    public const string CommentMetadata = "build_metadata.AdditionalFiles.IconMappingGenerator_Comment";

    public const string MarkerAttribute = """
        namespace IconMappingGenerator
        {
            [global::System.CodeDom.Compiler.GeneratedCode("IconMappingGenerator", "1.0.0.0")]
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public sealed class GenerateIconTypeMappingAttribute(string className) : global::System.Attribute
            {
                public string ClassName { get; } = className;
            }
        }
        """;

    public static SourceText IconType(
        string containingNamespace,
        string name,
        IReadOnlyList<IncludedIcon> embeddedIcons)
    {
        using var sourceCode = new StringWriter();
        var writer = new IndentedTextWriter(sourceCode);

        writer.WriteLine($"namespace {containingNamespace}");
        using (writer.BeginBlock())
        {
            var underlyingType = GetFittingType(embeddedIcons.Count);

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// All available icon types.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine(@"[global::System.CodeDom.Compiler.GeneratedCode(""IconMappingGenerator"", ""1.0.0.0"")]");
            writer.WriteLine($"public enum {name} : {underlyingType}");
            using (writer.BeginBlock())
            {
                foreach (var embeddedIcon in embeddedIcons)
                {
                    if (embeddedIcon.Comment is not null)
                    {
                        writer.WriteLine("/// <summary>");
                        writer.WriteLine($"/// {embeddedIcon.Comment}");
                        writer.WriteLine("/// </summary>");
                    }
                    writer.WriteLine($"{embeddedIcon.CodeName},");
                    writer.WriteLine();
                }
            }
        }

        writer.Flush();
        return SourceText.From(sourceCode.ToString(), Encoding.UTF8);
    }

    public static SourceText IconTypeMapping(
        string containingNamespace,
        string name,
        string iconTypeName,
        IReadOnlyList<IncludedIcon> embeddedIcons)
    {
        using var sourceCode = new StringWriter();
        var writer = new IndentedTextWriter(sourceCode);

        writer.WriteLine($"namespace {containingNamespace}");
        using (writer.BeginBlock())
        {
            writer.WriteLine("using System.Collections.Frozen;");
            writer.WriteLine();
            writer.WriteLine(@"[global::System.CodeDom.Compiler.GeneratedCode(""IconMappingGenerator"", ""1.0.0.0"")]");
            writer.WriteLine($"partial class {name}");
            using (writer.BeginBlock())
            {
                writer.WriteLine("/// <summary>");
                writer.WriteLine("/// Returns the assembly manifest name for the requested icon type.");
                writer.WriteLine("/// </summary>");
                writer.WriteLine($"private static readonly global::System.Collections.Frozen.FrozenDictionary<{iconTypeName}, string> ManifestResourceNames = new Dictionary<{iconTypeName}, string>");
                using (writer.BeginBlock())
                {
                    foreach (var embeddedIcon in embeddedIcons)
                    {
                        writer.WriteLine($"[{iconTypeName}.{embeddedIcon.CodeName}] = \"{embeddedIcon.ManifestName}\",");
                    }
                }
                writer.WriteLine(".ToFrozenDictionary();");
            }
        }

        writer.Flush();
        return SourceText.From(sourceCode.ToString(), Encoding.UTF8);
    }

    private static string GetFittingType(int iconCount)
    {
        return iconCount switch
        {
            < byte.MaxValue => "byte",
            < short.MaxValue => "short",
            < int.MaxValue => "int",
            _ => "long" // Just in case...
        };
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
