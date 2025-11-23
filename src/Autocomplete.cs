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
    /// Registers all executable files found in PATH environment variable
    /// </summary>
    public static void RegisterPathExecutables()
    {
        string? pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
        {
            return;
        }

        string[] paths = pathVariable.Split(Path.PathSeparator);

        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                continue;
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (!string.IsNullOrEmpty(fileName))
                {
                    Register(fileName);
                }
            }
        }
    }

    /// <summary>
    /// Gets the first suggestion for the given prefix or null
    /// </summary>
    public static List<string> GetSuggestion(string prefix)
    {
        var matches = _trie.GetAllMatchs(prefix).OrderByDescending(s => s).ToList();
        
        if (matches.Count == 0)
        {
            return matches;
        }

        // Check if one result is a prefix of all others
        foreach (var candidate in matches)
        {
            bool isCommonPrefix = true;
            foreach (var match in matches)
            {
                if (!match.StartsWith(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    isCommonPrefix = false;
                    break;
                }
            }

            if (isCommonPrefix)
            {
                return new List<string> { candidate };
            }
        }

        return matches;
    }
}
