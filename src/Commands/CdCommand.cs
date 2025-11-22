[CommandName("cd")]
public class CdCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            Console.WriteLine("cd: missing argument");
            return true;
        }

        string targetPath = args[0];

        // Handle tilde (~) for home directory
        if (targetPath.StartsWith("~"))
        {
            string? homeDir = Environment.GetEnvironmentVariable("HOME") 
                           ?? Environment.GetEnvironmentVariable("USERPROFILE");
            
            if (homeDir == null)
            {
                Console.WriteLine("cd: HOME environment variable not set");
                return true;
            }

            // Replace ~ with home directory path
            targetPath = targetPath.Length == 1 
                ? homeDir 
                : Path.Combine(homeDir, targetPath.Substring(2));
        }

        // Resolve relative paths (., .., etc.)
        try
        {
            // Get the full absolute path
            string absolutePath = Path.IsPathRooted(targetPath)
                ? targetPath
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), targetPath));

            // Check if directory exists
            if (!Directory.Exists(absolutePath))
            {
                Console.WriteLine($"cd: {targetPath}: No such file or directory");
                return true;
            }

            // Change to the directory
            Directory.SetCurrentDirectory(absolutePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"cd: {targetPath}: {ex.Message}");
        }

        return true;
    }
}