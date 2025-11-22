public class CommandExecutor
{
    private readonly Dictionary<string, ICommand> _commands;

    public CommandExecutor()
    {
        _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
        {
            { "exit", new ExitCommand() }
            // Add more built-in commands here as you create them
        };
    }

    /// <summary>
    /// Executes the given command
    /// </summary>
    /// <param name="command">The parsed command to execute</param>
    /// <returns>True if the shell should continue, false otherwise</returns>
    public bool Execute(Command command)
    {
        // Check if it's a built-in command
        if (_commands.TryGetValue(command.CommandName, out ICommand? commandInstance))
        {
            return commandInstance.Execute(command.Args);
        }

        // Command not found
        Console.WriteLine($"{command.CommandName}: command not found");
        return true; // Continue running the shell
    }
}
