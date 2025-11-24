[CommandName("false")]
public class FalseCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        Environment.ExitCode = 1;
        return true;
    }
}
