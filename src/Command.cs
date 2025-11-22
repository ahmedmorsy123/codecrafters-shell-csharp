public class Command
{
    private readonly string _commandName;
    private readonly List<string> _args;

    public Command(string commandName, params string[] args)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name cannot be null or empty", nameof(commandName));
        }

        _commandName = commandName;
        _args = new List<string>(args ?? Array.Empty<string>());
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string CommandName => _commandName;

    /// <summary>
    /// Gets the command arguments as a read-only list
    /// </summary>
    public IReadOnlyList<string> Args => _args;
}
