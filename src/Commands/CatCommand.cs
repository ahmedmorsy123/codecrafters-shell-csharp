[CommandName("cat")]
public class CatCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            // Read from stdin (for pipelines)
            string? inputLine;
            while ((inputLine = Console.ReadLine()) != null)
            {
                Console.WriteLine(inputLine);
            }

            return true;
        }
        foreach (var filePath in args)
        {
            if (File.Exists(filePath) == false)
            {
                Console.WriteLine($"cat: {filePath}: No such file or directory");
                continue;
            }

            try
            {
                var content = File.ReadAllText(filePath);
                if (!string.IsNullOrEmpty(content)) Console.WriteLine(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"cat: {filePath}: {ex.Message}");
            }
        }
        return true;
    }
}
