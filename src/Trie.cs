/// <summary>
/// Node used inside the Trie
/// </summary>
public class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; } = new();
    public bool IsEndOfWord { get; set; }
}

/// <summary>
/// Trie data structure for fast prefix autocomplete
/// </summary>
public class Trie
{
    private readonly TrieNode _root = new();

    /// <summary>
    /// Inserts a word into the Trie (case-insensitive)
    /// </summary>
    public void Insert(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return;

        var node = _root;
        var lower = word.ToLowerInvariant();

        foreach (char c in lower)
        {
            if (!node.Children.TryGetValue(c, out var child))
            {
                child = new TrieNode();
                node.Children[c] = child;
            }
            node = child;
        }

        node.IsEndOfWord = true;
    }

    /// <summary>
    /// Finds all words that start with the prefix.
    /// </summary>
    public List<string> FindAllWithPrefix(string prefix, int maxResults = 100)
    {
        var results = new List<string>();

        if (string.IsNullOrWhiteSpace(prefix))
            return results;

        var lower = prefix.ToLowerInvariant();
        var node = _root;

        // Navigate to end of the prefix
        foreach (char c in lower)
        {
            if (!node.Children.TryGetValue(c, out node))
                return results; // prefix not found
        }

        // Collect words from this node downward
        CollectWords(node, prefix.ToLowerInvariant(), results, maxResults);
        return results;
    }

    /// <summary>
    /// Recursively gathers words under a node
    /// </summary>
    private void CollectWords(TrieNode node, string current, List<string> results, int maxResults)
    {
        if (results.Count >= maxResults)
            return;

        if (node.IsEndOfWord)
            results.Add(current);

        foreach (var kvp in node.Children)
        {
            if (results.Count >= maxResults)
                break;

            CollectWords(kvp.Value, current + kvp.Key, results, maxResults);
        }
    }

    /// <summary>
    /// Gets the first autocomplete match for the prefix
    /// </summary>
    public string? GetFirstMatch(string prefix)
    {
        return FindAllWithPrefix(prefix, 1).FirstOrDefault();
    }

    public List<string> GetAllMatches(string prefix)
    {
        return FindAllWithPrefix(prefix);
    }

    /// <summary>
    /// Clears the trie
    /// </summary>
    public void Clear()
    {
        _root.Children.Clear();
        _root.IsEndOfWord = false;
    }
}
