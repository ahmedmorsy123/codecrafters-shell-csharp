[CommandName("alias")]
public class AliasCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        /*
        $ alias => show all aliases if no aliases no output
        $ alias tt => if there is an alias called tt show it if not dont't do an thing
        $ alias "tt=cat" => creates alias for cat
        $ alias "tt=cat" "mm=pwd" => create alias for cat and pwd
        */

        if (args.Count == 0)
        {
            foreach (var kvp in AliasManager.GetAll())
            {
                Console.WriteLine($"alias {kvp.Key}='{kvp.Value}'");
            }

            return true;
        }

        // if the args contain == so we create
        if (args.Any(arg => arg.Contains('=')))
        {
            foreach (string arg in args)
            {
                var parts = arg.Split('=');
                AliasManager.Set(parts[0], parts[1]);
            }

            return true;
        }


        foreach (string arg in args)
        {
            string? alias = AliasManager.Get(arg);
            if (alias is not null)
            {
                Console.WriteLine($"alias {arg}='{alias}'");
            }
        }



        return true;
    }
}
