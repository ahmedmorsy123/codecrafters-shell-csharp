[CommandName("sleep")]
public class SleepCommand : ICommand
{
    private const int MaxSleepSeconds = 3600; // Max 1 hour

    public bool Execute(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            Console.WriteLine("sleep: missing operand");
            return true;
        }
        if (!int.TryParse(args[0], out int seconds) || seconds < 0)
        {
            Console.WriteLine($"sleep: invalid time interval '{args[0]}'");
            return true;
        }

        if (seconds > MaxSleepSeconds)
        {
            Console.WriteLine($"sleep: invalid time interval '{args[0]}'");
            return true;
        }

        System.Threading.Thread.Sleep(seconds * 1000);

        return true;
    }
}
