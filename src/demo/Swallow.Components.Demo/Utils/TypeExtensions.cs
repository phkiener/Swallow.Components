using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Swallow.Components.Demo.Utils;

public static partial class TypeExtensions
{
    private static readonly FrozenDictionary<Type, string> primitiveTypeNames = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(nint), "nint" },
            { typeof(nuint), "nuint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(void), "void" },
        }
        .ToFrozenDictionary();

    public static string PrettyName(this Type type, bool isNullable = false)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType is not null)
        {
            return underlyingType.PrettyName(true);
        }

        if (type.IsGenericType)
        {
            var arguments = type.GetGenericArguments().Select(static t => t.PrettyName());
            var cleanedName = TypeParameterCountSuffix.Replace(type.Name, string.Empty);

            return $"{cleanedName}<{string.Join(", ", arguments)}>{(isNullable ? "?" : "")}";
        }

        var typeName = primitiveTypeNames.GetValueOrDefault(type) ?? type.Name;
        return $"{typeName}{(isNullable ? "?" : "")}";
    }

    [GeneratedRegex(@"`(\d+)$")]
    private static partial Regex TypeParameterCountSuffix { get; }
}
