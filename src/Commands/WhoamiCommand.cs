[CommandName("whoami")]
public class WhoamiCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        string? username = Environment.GetEnvironmentVariable("USERNAME")
                         ?? Environment.GetEnvironmentVariable("USER");
        if (!string.IsNullOrEmpty(username))
        {
            Console.WriteLine(username);
        }
        else
        {
            Console.WriteLine("whoami: cannot find name for user");
        }
        return true;
    }
}
