using System.Text.RegularExpressions;

namespace Unifintech.Infrastructure.Extensions;

public static class StringExtensions
{
    public static string ToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return Regex
            .Replace(value, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", "-$1", RegexOptions.Compiled)
            .ToLower();
    }
}
