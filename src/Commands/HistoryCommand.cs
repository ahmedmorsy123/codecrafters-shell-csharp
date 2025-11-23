[CommandName("history")]
public class HistoryCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        var historyEntries = PipelineHistory.ListHistory().ToList();
        int limit = int.TryParse(args.ElementAtOrDefault(0), out int parsedLimit) ? parsedLimit : 0;

        historyEntries = historyEntries.Skip(limit).ToList();

        if (historyEntries.Count == 0)
        {
            Console.WriteLine("No commands in history.");
        }
        else
        {
            for (int i = 0; i < historyEntries.Count; i++)
            {
                Console.WriteLine($"    {limit + i + 1}  {historyEntries[i]}");
            }
        }
        return true; // Continue running the shell
    }
}
