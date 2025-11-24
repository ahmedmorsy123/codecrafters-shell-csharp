[CommandName("unalias")]
public class UnaliasCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            Console.WriteLine("unalias: missing argument");
            return true;
        }

        if (args[0] == "-a")
        {
            AliasManager.RemoveAll();
            return true;
        }

        foreach (string arg in args)
        {
            if (AliasManager.Contains(arg))
            {
                AliasManager.Remove(arg);
            }
            else
            {
                Console.WriteLine($"unalias: {arg}: not found");
            }
        }
        return true;
    }
}
