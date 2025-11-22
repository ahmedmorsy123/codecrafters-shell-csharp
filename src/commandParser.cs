using System.Text;

public class CommandParser
{
    public static Command Parse(string commandLine)
    {
        commandLine = commandLine.Trim();
        
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            throw new ArgumentException("Command line cannot be empty");
        }

        List<string> parts = new List<string>();
        StringBuilder currentPart = new StringBuilder();
        bool inSingleQuote = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (c == '\'' )
            {
                // Toggle quote state, but don't include the quote character
                inSingleQuote = !inSingleQuote;
            }
            else if (c == ' ' && !inSingleQuote)
            {
                // Space outside quotes - end of current part
                if (currentPart.Length > 0)
                {
                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                }
                // Skip multiple consecutive spaces
            }
            else
            {
                // Regular character or space inside quotes
                currentPart.Append(c);
            }
        }

        // Add the last part if there is one
        if (currentPart.Length > 0)
        {
            parts.Add(currentPart.ToString());
        }

        // First part is the command name, rest are arguments
        string commandName = parts[0];
        string[] commandArgs = parts.Skip(1).ToArray();

        return new Command(commandName, commandArgs);
    }
}