using System.Text.RegularExpressions;

namespace DirectoryService.Shared;

public static partial class StringNormalization
{
    public static string Normalize(string value)
    {
        return SpaceRemoveRegex().Replace(value.Trim(), " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRemoveRegex();
}