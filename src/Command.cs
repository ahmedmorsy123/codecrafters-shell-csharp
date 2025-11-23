public class Command
{
    private readonly string _commandName;
    private readonly List<string> _args;
    private readonly RedirectionInfo _redirection;

    public Command(string commandName, string[] args, RedirectionInfo? redirection = null)
    {
        _commandName = commandName;
        _args = new List<string>(args ?? Array.Empty<string>());
        _redirection = redirection ?? new RedirectionInfo();
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string CommandName => _commandName;

    /// <summary>
    /// Gets the command arguments as a read-only list
    /// </summary>
    public IReadOnlyList<string> Args => _args;

    /// <summary>
    /// Gets the redirection information
    /// </summary>
    public RedirectionInfo Redirection => _redirection;

    public override string ToString()
    {
        var argsString = string.Join(" ", _args);
        return $"{_commandName} {argsString}".Trim();
    }
}

