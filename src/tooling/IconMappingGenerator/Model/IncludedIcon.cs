using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IconMappingGenerator;

internal readonly struct IncludedIcon(string iconName, string manifestName, string codeName, string? comment)
{
    public string IconName { get; } = iconName;
    public string ManifestName { get; } = manifestName;
    public string CodeName { get; } = codeName;
    public string? Comment { get; } = comment;

    public static bool IsRelevant(AdditionalText additionalText, AnalyzerConfigOptionsProvider optionsProvider)
    {
        var fileOptions = optionsProvider.GetOptions(additionalText);

        return fileOptions.TryGetValue(SourceCodes.IncludeIconMetadata, out var metadataValue)
               && metadataValue.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
               && fileOptions.TryGetValue(SourceCodes.ResourceNameMetadata, out var manifestValue)
               && !string.IsNullOrWhiteSpace(manifestValue);
    }

    public static IncludedIcon CreateFrom(AdditionalText additionalText, AnalyzerConfigOptionsProvider optionsProvider)
    {
        var filename = Path.GetFileNameWithoutExtension(additionalText.Path);
        var commentText = optionsProvider.GetOptions(additionalText).TryGetValue(SourceCodes.CommentMetadata, out var commentMetadata)
            ? commentMetadata
            : null;

        var manifestName = optionsProvider.GetOptions(additionalText).TryGetValue(SourceCodes.ResourceNameMetadata, out var manifestMetadata)
            ? manifestMetadata
            : throw new InvalidOperationException();

        return new IncludedIcon(filename, manifestName, ToTitleCase(filename), commentText);
    }

    private static string ToTitleCase(string text)
    {
        var container = new StringBuilder(capacity: text.Length);

        var makeUpper = true; // first character should always be uppercased.
        foreach (var character in text)
        {
            if (character is '-')
            {
                makeUpper = true;
                continue;
            }

            container.Append(makeUpper ? char.ToUpperInvariant(character) : character);
            makeUpper = false;
        }

        return container.ToString();
    }
}
