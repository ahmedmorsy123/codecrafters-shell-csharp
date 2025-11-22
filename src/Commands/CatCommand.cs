using System.Text;

[CommandName("cat")]
public class CatCommand : ICommand
{
    public bool Execute(IReadOnlyList<string> args)
    {
        foreach (string path in args)
        {
            byte[] fileContent = File.ReadAllBytes(path);
            string content = Encoding.UTF8.GetString(fileContent);
            Console.Write(content);
        }

        return true;
    }
}
