[CommandName("which")]
public class WhichCommand : ICommand
{
    private static readonly string[] WindowsExecutableExtensions =
        Environment.GetEnvironmentVariable("PATHEXT")?.Split(';')
        ?? new[] { ".exe", ".bat", ".cmd", ".com" };

    public bool Execute(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            Console.WriteLine("which: missing argument");
            return true;
        }

        string? pathVariable = Environment.GetEnvironmentVariable("PATH");
        string[] paths = pathVariable?.Split(Path.PathSeparator) ?? Array.Empty<string>();

        foreach (var cmdName in args)
        {
            if (CommandExecutor.IsBuiltInCommand(cmdName))
            {
                Console.WriteLine($"{cmdName} is a shell builtin");
                continue;
            }

            bool found = false;
            foreach (string path in paths)
            {
                // Try exact match first
                string fullPath = Path.Combine(path, cmdName);
                if (File.Exists(fullPath))
                {
                    Console.WriteLine(fullPath);
                    found = true;
                    break;
                }

                // On Windows, try with executable extensions
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    foreach (var ext in WindowsExecutableExtensions)
                    {
                        string fullPathWithExt = Path.Combine(path, cmdName + ext);
                        if (File.Exists(fullPathWithExt))
                        {
                            Console.WriteLine(fullPathWithExt);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }
        }
        return true;
    }
}
