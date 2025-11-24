[CommandName("true")]
public class TrueCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        return true;
    }
}
