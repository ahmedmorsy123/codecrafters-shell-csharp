[CommandName("clear")]
[CommandName("cls")]
public class ClearCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        Console.Clear();
        return true; // Continue running the shell
    }
}