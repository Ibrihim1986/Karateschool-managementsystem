using System.Text.RegularExpressions;

namespace KarateSchool.Tests.Integration;

internal static class AntiForgeryHelper
{
    private static readonly Regex TokenPattern = new(
        @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
        RegexOptions.Compiled);

    public static async Task<string> ExtractTokenAsync(HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var match = TokenPattern.Match(html);
        if (!match.Success)
            throw new InvalidOperationException("Antiforgery token not found in response.");
        return match.Groups[1].Value;
    }
}
