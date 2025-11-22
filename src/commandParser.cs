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
        bool inDoubleQuote = false;
        bool escaped = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (escaped)
            {
                // Previous character was a backslash
                if (inDoubleQuote)
                {
                    // In double quotes: only ", \, $, `, and newline can be escaped
                    // For this stage, we only handle " and \
                    if (c == '"' || c == '\\')
                    {
                        currentPart.Append(c);
                    }
                    else
                    {
                        // Backslash is literal for other characters
                        currentPart.Append('\\');
                        currentPart.Append(c);
                    }
                }
                else if (inSingleQuote)
                {
                    // In single quotes: backslash has no special meaning
                    // This shouldn't happen because we don't set escaped=true in single quotes
                    currentPart.Append('\\');
                    currentPart.Append(c);
                }
                else
                {
                    // Outside quotes: backslash escapes the next character
                    currentPart.Append(c);
                }
                escaped = false;
            }
            else if (c == '\\')
            {
                // Backslash encountered
                if (inSingleQuote)
                {
                    // In single quotes: backslash is literal
                    currentPart.Append(c);
                }
                else
                {
                    // Outside quotes or in double quotes: mark as escaped
                    escaped = true;
                }
            }
            else if (c == '\'' && !inDoubleQuote)
            {
                // Toggle single quote state (only if not inside double quotes)
                inSingleQuote = !inSingleQuote;
            }
            else if (c == '"' && !inSingleQuote)
            {
                // Toggle double quote state (only if not inside single quotes)
                inDoubleQuote = !inDoubleQuote;
            }
            else if (c == ' ' && !inSingleQuote && !inDoubleQuote)
            {
                // Space outside all quotes - end of current part
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

        // Handle trailing backslash (if any)
        if (escaped)
        {
            currentPart.Append('\\');
        }

        // Add the last part if there is one
        if (currentPart.Length > 0)
        {
            parts.Add(currentPart.ToString());
        }

        if (parts.Count == 0)
        {
            throw new ArgumentException("No command found");
        }

        // First part is the command name (can be quoted), rest are arguments
        string commandName = parts[0];
        string[] commandArgs = parts.Skip(1).ToArray();

        return new Command(commandName, commandArgs);
    }
}