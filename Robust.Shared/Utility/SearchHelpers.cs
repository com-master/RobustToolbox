using System;
using System.Collections.Generic;

namespace Robust.Shared.Utility;

/// <summary>
///     Helpers for user-facing text search that fold interchangeable characters together, so that a
///     search finds results regardless of which variant the player happened to type.
/// </summary>
/// <remarks>
///     Some languages have letters that players treat as interchangeable when typing. For example, in
///     Russian the letters 'е' and 'ё' are routinely used interchangeably, so searching for one should
///     also match the other. The default equivalences cover such cases; games or localizations that
///     need more can register additional pairs with <see cref="AddEquivalence"/> at startup.
/// </remarks>
public static class SearchHelpers
{
    // Maps a character to the canonical character it is folded to when normalizing text for search.
    // Not thread-safe to modify: register equivalences during startup, before any search runs.
    private static readonly Dictionary<char, char> Equivalences = new()
    {
        // Russian: 'ё' is commonly typed (and omitted) as 'е'.
        ['ё'] = 'е',
        ['Ё'] = 'Е',
    };

    /// <summary>
    ///     Registers an additional character equivalence for search: occurrences of
    ///     <paramref name="from"/> are treated as <paramref name="to"/> when normalizing text.
    ///     Call this during startup, before searches run, as the equivalence table is not thread-safe.
    /// </summary>
    public static void AddEquivalence(char from, char to)
    {
        Equivalences[from] = to;
    }

    /// <summary>
    ///     Determines whether <paramref name="source"/> contains <paramref name="search"/>, folding
    ///     any registered interchangeable characters together before comparing.
    /// </summary>
    /// <param name="source">The text being searched.</param>
    /// <param name="search">The text to look for.</param>
    /// <param name="comparison">The string comparison rules to use for the rest of the comparison.</param>
    public static bool ContainsSearch(this string source, string search, StringComparison comparison)
    {
        return NormalizeForSearch(source).Contains(NormalizeForSearch(search), comparison);
    }

    /// <summary>
    ///     Folds any registered interchangeable characters in <paramref name="value"/> to their
    ///     canonical form. Returns the original string instance when nothing needs changing.
    /// </summary>
    public static string NormalizeForSearch(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        char[]? buffer = null;
        for (var i = 0; i < value.Length; i++)
        {
            if (!Equivalences.TryGetValue(value[i], out var replacement))
                continue;

            buffer ??= value.ToCharArray();
            buffer[i] = replacement;
        }

        return buffer == null ? value : new string(buffer);
    }
}
