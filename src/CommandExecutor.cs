public class CommandExecutor
{
    private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);

    public CommandExecutor()
    {
        RegisterCommands();
    }

    /// <summary>
    /// Automatically discovers and registers all commands using reflection
    /// </summary>
    private void RegisterCommands()
    {
        // Get all types in the current assembly
        var commandTypes = typeof(CommandExecutor).Assembly.GetTypes()
            .Where(type => typeof(ICommand).IsAssignableFrom(type) 
                        && !type.IsInterface 
                        && !type.IsAbstract);

        foreach (var commandType in commandTypes)
        {
            // Look for CommandName attributes
            var attributes = commandType.GetCustomAttributes(typeof(CommandNameAttribute), false)
                .Cast<CommandNameAttribute>();

            if (!attributes.Any())
            {
                // Skip commands without the attribute
                continue;
            }

            // Create an instance of the command
            var commandInstance = (ICommand)Activator.CreateInstance(commandType)!;

            // Register each command name from the attributes
            foreach (var attribute in attributes)
            {
                if (_commands.ContainsKey(attribute.Name))
                {
                    Console.WriteLine($"Warning: Duplicate command name '{attribute.Name}' ignored for {commandType.Name}");
                    continue;
                }

                _commands[attribute.Name] = commandInstance;
            }
        }
    }

    /// <summary>
    /// Checks if a command is registered as a built-in command
    /// </summary>
    /// <param name="commandName">The command name to check</param>
    /// <returns>True if the command is registered, false otherwise</returns>
    public static bool IsCommand(string commandName)
    {
        return _commands.ContainsKey(commandName);
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
