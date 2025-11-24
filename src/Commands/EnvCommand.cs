[CommandName("env")]
public class EnvCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .Where(de => args.Count == 0 || args.Any(arg => arg.Equals((string)de.Key, StringComparison.OrdinalIgnoreCase)))
            .ToList()
            .ForEach(de => Console.WriteLine($"{de.Key}={de.Value}"));

        return true;
    }
}
