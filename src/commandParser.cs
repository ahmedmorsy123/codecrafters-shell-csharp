
public class CommandParser
{
    public static Command Parse(string commandLine)
    {
        string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string commandName = parts[0];
        string[] commandArgs = parts.Skip(1).ToArray();

        return new Command(commandName, commandArgs);
    }
}