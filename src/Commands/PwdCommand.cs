[CommandName("pwd")]
public class PwdCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
        return true;
    }
}