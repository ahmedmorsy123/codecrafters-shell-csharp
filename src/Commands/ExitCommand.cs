public class ExitCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        // Exit command with optional exit code
        int exitCode = 0;
        
        if (args.Count > 0 && int.TryParse(args[0], out int parsedCode))
        {
            exitCode = parsedCode;
        }
        
        Environment.Exit(exitCode);
        
        // This line will never be reached, but needed for interface contract
        return false;
    }
}
