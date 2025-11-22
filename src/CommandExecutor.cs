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

        // Try to find and execute as external command
        if (TryExecuteExternal(command))
        {
            return true;
        }

        // Command not found
        Console.WriteLine($"{command.CommandName}: command not found");
        return true; // Continue running the shell
    }

    /// <summary>
    /// Attempts to find and execute an external command from PATH
    /// </summary>
    private bool TryExecuteExternal(Command command)
    {
        string? executablePath = FindExecutableInPath(command.CommandName);
        
        if (executablePath == null)
        {
            return false;
        }

        try
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };


            foreach (var arg in command.Args)
            {
                processInfo.ArgumentList.Add(arg);
            }

            using var process = System.Diagnostics.Process.Start(processInfo);
            
            if (process == null)
            {
                return false;
            }

            // Read output and error streams
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // Print output
            if (!string.IsNullOrEmpty(output))
            {
                Console.Write(output);
            }

            // Print errors
            if (!string.IsNullOrEmpty(error))
            {
                Console.Error.Write(error);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finds an executable in the PATH environment variable
    /// </summary>
    public static string? FindExecutableInPath(string executableName)
    {
        string? pathVariable = Environment.GetEnvironmentVariable("PATH");
        
        if (pathVariable == null)
        {
            return null;
        }
        
        string[] paths = pathVariable.Split(Path.PathSeparator);
        
        foreach (string path in paths)
        {
            // Skip if directory doesn't exist
            if (!Directory.Exists(path))
            {
                continue;
            }

            string candidatePath = Path.Combine(path, executableName);
            
            // Check if file exists and has execute permission
            if (File.Exists(candidatePath) && HasExecutePermission(candidatePath))
            {
                return candidatePath;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a file has execute permissions
    /// </summary>
    public static bool HasExecutePermission(string filePath)
    {
        try
        {
            // For Unix-like systems
            if (!OperatingSystem.IsWindows())
            {
                var mode = File.GetUnixFileMode(filePath);
                return (mode & (UnixFileMode.UserExecute | 
                            UnixFileMode.GroupExecute | 
                            UnixFileMode.OtherExecute)) != 0;
            }
            
            // For Windows - just check if file exists
            return File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }
}
