[CommandName("history")]
public class HistoryCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        var historyEntries = PipelineHistory.ListHistory().ToList();
        int limit = int.TryParse(args.ElementAtOrDefault(0), out int parsedLimit) ? parsedLimit : 0;

        // If limit is specified, take only the last N entries
        if (limit > 0 && limit < historyEntries.Count)
        {
            historyEntries = historyEntries.Skip(historyEntries.Count - limit).ToList();
        }

        if (historyEntries.Count == 0)
        {
            Console.WriteLine("No commands in history.");
        }
        else
        {
            for (int i = 0; i < historyEntries.Count; i++)
            {
                Console.WriteLine($"    {historyEntries[i].position}  {historyEntries[i].entry}");
            }
        }
        return true; // Continue running the shell
    }
}
