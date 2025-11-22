[CommandName("type")]
public class TypeCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            Console.WriteLine("type: missing argument");
            return true;
        }

        string commandName = args[0];

        if (CommandExecutor.IsCommand(commandName))
        {
            Console.WriteLine($"{commandName} is a shell builtin");
        }
        else if (FindExecutableInPath(commandName, out string? fullPath))
        {
            Console.WriteLine($"{commandName} is {fullPath}");
        }
        else
        {
            Console.WriteLine($"{commandName}: not found");
        }

        return true; // Continue running the shell
    }

    private static bool FindExecutableInPath(string executableName, out string? fullPath)
    {
        string? pathVariable = Environment.GetEnvironmentVariable("PATH");
        
        if (pathVariable == null)
        {
            fullPath = null;
            return false;
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
                fullPath = candidatePath;
                return true;
            }
        }

        fullPath = null;
        return false;
    }

    private static bool HasExecutePermission(string filePath)
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