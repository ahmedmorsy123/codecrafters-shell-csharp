using System.Collections.Generic;

/// <summary>
/// Static manager for command aliases, similar to PipelineHistory.
/// Stores alias name to command value mappings.
/// </summary>
public static class AliasManager
{
    private static readonly Dictionary<string, string> _aliases = new();

    /// <summary>
    /// Add or update an alias.
    /// </summary>
    public static void Set(string name, string value)
    {
        _aliases[name] = value;
    }

    /// <summary>
    /// Get an alias value by name. Returns null if not found.
    /// </summary>
    public static string? Get(string name)
    {
        return _aliases.TryGetValue(name, out var value) ? value : null;
    }

    /// <summary>
    /// Check if an alias exists.
    /// </summary>
    public static bool Contains(string name)
    {
        return _aliases.ContainsKey(name);
    }

    /// <summary>
    /// Remove an alias by name.
    /// </summary>
    public static bool Remove(string name)
    {
        return _aliases.Remove(name);
    }

    public static void RemoveAll()
    {
        _aliases.Clear();
    }

    /// <summary>
    /// Get all aliases.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetAll()
    {
        return _aliases;
    }

    /// <summary>
    /// Clear all aliases. Essential for tests.
    /// </summary>
    public static void Clear()
    {
        _aliases.Clear();
    }
}
