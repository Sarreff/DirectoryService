using System.Text.RegularExpressions;

namespace DirectoryService.Domain.Shared;

internal static partial class IdentifierRegex
{
    [GeneratedRegex(@"^[A-Za-z\s-]+$")]
    public static partial Regex LatinName();
}