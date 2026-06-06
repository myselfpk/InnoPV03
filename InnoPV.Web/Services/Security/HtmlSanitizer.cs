using System.Text.RegularExpressions;

namespace InnoPV.Web.Services.Security;

public static class HtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "b", "strong", "i", "em", "u", "p", "br", "ul", "ol", "li", "div", "span"
    };

    public static string? Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html;
        }

        var sanitized = Regex.Replace(
            html,
            @"<!--.*?-->|<(script|style|iframe|object|embed|svg|math)\b[^>]*>.*?</\1>",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        return Regex.Replace(
            sanitized,
            @"</?([a-zA-Z][a-zA-Z0-9]*)(?:\s[^>]*)?>",
            match =>
            {
                var tagName = match.Groups[1].Value;

                if (!AllowedTags.Contains(tagName))
                {
                    return string.Empty;
                }

                if (tagName.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    return "<br>";
                }

                return match.Value.StartsWith("</", StringComparison.Ordinal)
                    ? $"</{tagName.ToLowerInvariant()}>"
                    : $"<{tagName.ToLowerInvariant()}>";
            },
            RegexOptions.IgnoreCase);
    }
}
