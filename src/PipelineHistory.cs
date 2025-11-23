public class PipelineHistory
{
    private static readonly List<(Pipeline pipeline, int position)> _history = new List<(Pipeline, int)>();
    private static int _currentIndex = -1;

    /// <summary>
    /// Adds a pipeline to the history.
    /// </summary>
    public static void Add(Pipeline pipeline)
    {
        if (pipeline is not null)
        {
            _history.Add((pipeline, _history.Count + 1));
            _currentIndex = _history.Count; // Reset index to the end
        }
    }

    /// <summary>
    /// Gets the previous pipeline in history.
    /// </summary>
    public static Pipeline? GetPrevious()
    {
        if (_history.Count == 0 || _currentIndex <= 0)
            return null;

        _currentIndex--;
        return _history[_currentIndex].pipeline;
    }

    /// <summary>
    /// Gets the next pipeline in history.
    /// </summary>
    public static Pipeline? GetNext()
    {
        if (_history.Count == 0 || _currentIndex >= _history.Count - 1)
            return null;

        _currentIndex++;
        return _history[_currentIndex].pipeline;
    }

    /// <summary>
    /// List all commands in history with there args.
    /// </summary>
    public static IEnumerable<(string entry, int position)> ListHistory()
    {
        foreach (var entry in _history)
        {
            if (entry.pipeline == null) continue;
            yield return (entry.pipeline.ToString(), entry.position);
        }
    }

    /// <summary>
    /// Read history at startup from the file provided by HISTFILE environment variable
    /// </summary>
    public static void LoadHistoryFromFile()
    {
        string? histFilePath = Environment.GetEnvironmentVariable("HISTFILE");
        if (string.IsNullOrEmpty(histFilePath))
        {
            return;
        }

        // Expand ~ to home directory if needed
        if (histFilePath.StartsWith("~/") || histFilePath.StartsWith("~\\"))
        {
            string? home = Environment.GetEnvironmentVariable("HOME")
                ?? Environment.GetEnvironmentVariable("USERPROFILE");
            if (!string.IsNullOrEmpty(home))
            {
                histFilePath = Path.Combine(home, histFilePath.Substring(2));
            }
        }

        // Convert to absolute path if relative
        if (!Path.IsPathRooted(histFilePath))
        {
            histFilePath = Path.GetFullPath(histFilePath);
        }

        if (!File.Exists(histFilePath))
        {
            return;
        }

        try
        {
            var lines = File.ReadAllLines(histFilePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var pipeline = CommandParser.ParsePipeline(line);
                    Add(pipeline);
                }
            }
        }
        catch
        {
            // Ignore errors reading history file
        }
    }

    /// <summary>
    /// Write history to the file provided by HISTFILE environment variable at shell exit
    /// </summary>
    public static void SaveHistoryToFile()
    {
        string? histFilePath = Environment.GetEnvironmentVariable("HISTFILE");
        if (string.IsNullOrEmpty(histFilePath))
        {
            return;
        }

        // Expand ~ to home directory if needed
        if (histFilePath.StartsWith("~/") || histFilePath.StartsWith("~\\"))
        {
            string? home = Environment.GetEnvironmentVariable("HOME")
                ?? Environment.GetEnvironmentVariable("USERPROFILE");
            if (!string.IsNullOrEmpty(home))
            {
                histFilePath = Path.Combine(home, histFilePath.Substring(2));
            }
        }

        // Convert to absolute path if relative
        if (!Path.IsPathRooted(histFilePath))
        {
            histFilePath = Path.GetFullPath(histFilePath);
        }

        try
        {
            // Create directory if it doesn't exist
            string? directory = Path.GetDirectoryName(histFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var historyEntries = ListHistory();
            File.WriteAllLines(histFilePath, historyEntries.Select(entry => entry.entry));
        }
        catch
        {
            // Ignore errors writing history file
        }
    }

    public static void ClearHistory()
    {
        _history.Clear();
        _currentIndex = -1;
    }
}