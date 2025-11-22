[CommandName("echo")]
public class EchoCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        // Echo command
        Console.WriteLine(string.Join(" ", args));
        return true; // Continue running the shell
    }
}