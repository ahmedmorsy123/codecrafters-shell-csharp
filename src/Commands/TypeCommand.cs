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
        else
        {
            Console.WriteLine($"{commandName}: not found");
        }

        return true; // Continue running the shell
    }
}