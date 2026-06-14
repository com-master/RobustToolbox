using System;

namespace Robust.Shared.Utility;

/// <summary>
///     Helpers for user-facing text search that should be lenient about certain character
///     equivalences, such as treating the Cyrillic letters 'е' and 'ё' as the same letter.
/// </summary>
public static class SearchHelpers
{
    /// <summary>
    ///     Determines whether <paramref name="source"/> contains <paramref name="search"/>,
    ///     treating the Cyrillic letters 'е' and 'ё' (and their uppercase forms) as equivalent.
    /// </summary>
    /// <param name="source">The text being searched.</param>
    /// <param name="search">The text to look for.</param>
    /// <param name="comparison">The string comparison rules to use for the rest of the comparison.</param>
    public static bool ContainsYoInsensitive(this string source, string search, StringComparison comparison)
    {
        return NormalizeYo(source).Contains(NormalizeYo(search), comparison);
    }

    /// <summary>
    ///     Replaces the Cyrillic letter 'ё'/'Ё' with 'е'/'Е' so the two are treated as equivalent
    ///     during text search. Returns the original string instance when nothing needs replacing.
    /// </summary>
    public static string NormalizeYo(string value)
    {
        if (value.IndexOf('ё') < 0 && value.IndexOf('Ё') < 0)
            return value;

        return value.Replace('ё', 'е').Replace('Ё', 'Е');
    }
}
