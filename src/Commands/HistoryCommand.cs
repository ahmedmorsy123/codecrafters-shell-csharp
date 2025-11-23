
[CommandName("history")]
public class HistoryCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        // history command can be run by one of these ways:
        // 1- history [limit] => should return the last N entries
        // 2- history -r <path_to_history_file> => to read the history from a file
        // 3- history -w <path_to_history_file> => to write the history to a file
        // 4- history -a <path_to_history_file> => to append the history to a file
        // and we also can read/write/append from/to the default history file that is the HISTFILE environment variable

        if (args.Count() == 0)
        {
            return GetHistoryWithLimit(0);
        }

        switch (args[0])
        {
            case "-r":
                return ReadHistoryFromFile(args[1]);
            case "-w":
                return WriteHistoryToFile(args[1]);
            case "-a":
                return AppendHistoryToFile(args[1]);
            case var s when int.TryParse(s, out int limit):
                return GetHistoryWithLimit(limit);
            default:
                return GetHistoryWithLimit(0);
        }
    }

    private bool AppendHistoryToFile(string filePath)
    {
        if (filePath == null)
        {
            Console.WriteLine("No file path provided for appending history.");
            return false;
        }

        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        var historyEntries = PipelineHistory.ListHistory();
        File.AppendAllLines(filePath, historyEntries.Select(entry => entry.entry));
        PipelineHistory.ClearHistory();
        return true;
    }

    private bool WriteHistoryToFile(string filePath)
    {
        if (filePath == null)
        {
            Console.WriteLine("No file path provided for writing history.");
            return false;
        }

        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }

        var historyEntries = PipelineHistory.ListHistory();
        File.WriteAllLines(filePath, historyEntries.Select(entry => entry.entry));
        return true;
    }

    private bool ReadHistoryFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"History file '{filePath}' does not exist.");
            return false;
        }

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var pipeline = CommandParser.ParsePipeline(line); // this will convert the line to a pipeline and add it to history
        }

        return true;
    }

    private bool GetHistoryWithLimit(int limit)
    {
        var historyEntries = PipelineHistory.ListHistory().ToList();

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
        return true;
    }
}
