using System.Text.RegularExpressions;

public static class URLSlugService
{
    public static string ToSlug(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string slug = input.ToLowerInvariant();

        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");

        return slug.Trim('-');
    }
}
