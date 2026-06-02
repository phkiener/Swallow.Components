namespace Swallow.Components.Utils.Internal;

internal static class AttributeHelper
{
    public static string Class(IReadOnlyDictionary<string, object?>? attributes, params IEnumerable<string?> classes)
    {
        if (attributes is null || !attributes.TryGetValue("class", out var givenClass) || givenClass is null)
        {
            return string.Join(" ", classes);
        }

        return string.Join(" ", [givenClass.ToString(), ..classes]);
    }
}
