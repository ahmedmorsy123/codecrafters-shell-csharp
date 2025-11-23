using System.Collections.Generic;

/// <summary>
/// Small wrapper around Trie to own autocomplete responsibilities.
/// </summary>
public static class Autocomplete
{
    private static readonly Trie _trie = new Trie();

    /// <summary>
    /// Register a word for autocomplete
    /// </summary>
    public static void Register(string word)
    {
        _trie.Insert(word);
    }

    /// <summary>
    /// Register multiple words
    /// </summary>
    public static void RegisterMany(IEnumerable<string> words)
    {
        foreach (var w in words)
            Register(w);
    }

    /// <summary>
    /// Gets the first suggestion for the given prefix or null
    /// </summary>
    public static string? GetSuggestion(string prefix)
    {
        return _trie.GetFirstMatch(prefix);
    }
}
