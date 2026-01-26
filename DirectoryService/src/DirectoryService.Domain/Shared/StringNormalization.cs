using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Shared;

internal static partial class StringNormalization
{
    public static string Normalize(string value)
    {
        return SpaceRemoveRegex().Replace(value.Trim(), " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRemoveRegex();
}