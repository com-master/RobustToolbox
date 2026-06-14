using System;
using System.Collections.Generic;
using Robust.Shared.Configuration;

namespace Robust.Shared.Utility;

/// <summary>
///     Helpers for user-facing text search that fold interchangeable characters together, so that a
///     search finds results regardless of which variant the player happened to type.
/// </summary>
/// <remarks>
///     Some languages have letters that players treat as interchangeable when typing. For example, in
///     Russian the letters 'е' and 'ё' are routinely used interchangeably, so searching for one should
///     also match the other. The set of equivalent characters is configured through the
///     <c>interface.search_char_equivalences</c> CVar (see <see cref="CVars.SearchCharEquivalences"/>),
///     so games or localizations can adjust it for other languages without code changes.
/// </remarks>
public static class SearchHelpers
{
    // Maps a character to the canonical character it is folded to when normalizing text for search.
    // Replaced wholesale (atomic reference assignment) whenever the configured value changes.
    private static IReadOnlyDictionary<char, char> _equivalences = new Dictionary<char, char>();
    private static bool _initialized;

    /// <summary>
    ///     Hooks the search equivalence table up to configuration. Safe to call multiple times; only
    ///     the first call subscribes. Call this during startup before searches run.
    /// </summary>
    public static void EnsureInitialized(IConfigurationManager cfg)
    {
        if (_initialized)
            return;

        _initialized = true;
        cfg.OnValueChanged(CVars.SearchCharEquivalences, ParseEquivalences, invokeImmediately: true);
    }

    /// <summary>
    ///     Determines whether <paramref name="source"/> contains <paramref name="search"/>, folding
    ///     any configured interchangeable characters together before comparing.
    /// </summary>
    /// <param name="source">The text being searched.</param>
    /// <param name="search">The text to look for.</param>
    /// <param name="comparison">The string comparison rules to use for the rest of the comparison.</param>
    public static bool ContainsSearch(this string source, string search, StringComparison comparison)
    {
        return NormalizeForSearch(source).Contains(NormalizeForSearch(search), comparison);
    }

    /// <summary>
    ///     Folds any configured interchangeable characters in <paramref name="value"/> to their
    ///     canonical form. Returns the original string instance when nothing needs changing.
    /// </summary>
    public static string NormalizeForSearch(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var table = _equivalences;
        if (table.Count == 0)
            return value;

        char[]? buffer = null;
        for (var i = 0; i < value.Length; i++)
        {
            if (!table.TryGetValue(value[i], out var replacement))
                continue;

            buffer ??= value.ToCharArray();
            buffer[i] = replacement;
        }

        return buffer == null ? value : new string(buffer);
    }

    // Parses the comma-separated "from/to" pairs from the CVar into a fresh lookup table.
    private static void ParseEquivalences(string raw)
    {
        var table = new Dictionary<char, char>();

        foreach (var pair in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // Each pair must be exactly two characters: the typed variant and the canonical form.
            if (pair.Length == 2)
                table[pair[0]] = pair[1];
        }

        _equivalences = table;
    }
}
