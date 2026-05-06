using System.Text.RegularExpressions;

namespace BeautyPlus.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = text.ToLowerInvariant().Trim();
        text = Regex.Replace(text, @"[^a-z0-9\s\-]", string.Empty);
        text = Regex.Replace(text, @"\s+", "-");
        text = Regex.Replace(text, @"\-{2,}", "-");

        return text.Trim('-');
    }
}