using IconMappingGenerator;

namespace Swallow.Components.Icons;

[GenerateIconTypeMapping("IconType")]
internal static partial class IconTypeMapping
{
    public static string ManifestPathFor(IconType iconType) => ManifestResourceNames[iconType];
}
