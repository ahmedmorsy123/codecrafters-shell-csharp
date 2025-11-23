[CommandName("exit")]
public class ExitCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        // Exit command with optional exit code
        // Return false to signal the shell should exit gracefully
        // The exit code is currently not used, but could be stored for later use
        return false;
    }
}
