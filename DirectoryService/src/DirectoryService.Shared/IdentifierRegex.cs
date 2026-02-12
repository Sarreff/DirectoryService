using System.Text.RegularExpressions;

namespace DirectoryService.Shared;

public static partial class IdentifierRegex
{
    [GeneratedRegex(@"^[A-Za-z\s-]+$")]
    public static partial Regex LatinName();
}