public class PipelineHistory
{
    private static readonly List<Pipeline> _history = new List<Pipeline>();
    private static int _currentIndex = -1;

    /// <summary>
    /// Adds a pipeline to the history.
    /// </summary>
    public static void Add(Pipeline pipeline)
    {
        if (pipeline is not null)
        {
            _history.Add(pipeline);
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
        return _history[_currentIndex];
    }

    /// <summary>
    /// Gets the next pipeline in history.
    /// </summary>
    public static Pipeline? GetNext()
    {
        if (_history.Count == 0 || _currentIndex >= _history.Count - 1)
            return null;

        _currentIndex++;
        return _history[_currentIndex];
    }

    /// <summary>
    /// List all commands in history with there args.
    /// </summary>
    public static IEnumerable<string> ListHistory()
    {
        foreach (var pipeline in _history)
        {
            if (pipeline == null) continue;
            yield return pipeline.ToString();
        }
    }

}