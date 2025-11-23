using System.Text;
using System.Collections.Generic;
using System.Linq;

public class CommandParser
{
    /// <summary>
    /// Parses a command line that may contain pipes
    /// </summary>
    public static Pipeline ParsePipeline(string commandLine)
    {
        // Split by pipe character (|) while respecting quotes
        List<string> segments = SplitByPipe(commandLine);

        List<Command> commands = new List<Command>();
        foreach (string segment in segments)
        {
            commands.Add(Parse(segment));
        }

        return new Pipeline(commands);
    }

    /// <summary>
    /// Splits command line by pipe (|) while respecting quotes
    /// </summary>
    private static List<string> SplitByPipe(string commandLine)
    {
        List<string> segments = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inSingleQuote = false;
        bool inDoubleQuote = false;
        bool escaped = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];

            if (escaped)
            {
                current.Append(c);
                escaped = false;
            }
            else if (c == '\\')
            {
                if (inSingleQuote)
                {
                    current.Append(c);
                }
                else
                {
                    escaped = true;
                    current.Append(c);
                }
            }
            else if (c == '\'' && !inDoubleQuote)
            {
                inSingleQuote = !inSingleQuote;
                current.Append(c);
            }
            else if (c == '"' && !inSingleQuote)
            {
                inDoubleQuote = !inDoubleQuote;
                current.Append(c);
            }
            else if (c == '|' && !inSingleQuote && !inDoubleQuote)
            {
                // Pipe found outside quotes - this is a pipeline separator
                segments.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        // Add the last segment
        if (current.Length > 0 || segments.Count > 0)
        {
            segments.Add(current.ToString());
        }

        return segments;
    }

    /// <summary>
    /// Parses a single command (without pipes)
    /// </summary>
    public static Command Parse(string commandLine)
    {
        commandLine = commandLine.Trim();

        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return new Command("", new string[0]);
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

        // Parse redirections from the parts
        RedirectionInfo redirection = new RedirectionInfo();
        List<string> filteredParts = new List<string>();

        for (int i = 0; i < parts.Count; i++)
        {
            string part = parts[i];

            // Check for redirection operators
            if (part == ">" || part == "1>")
            {
                // Stdout redirection (overwrite)
                if (i + 1 < parts.Count)
                {
                    redirection.StdoutFile = parts[i + 1];
                    redirection.StdoutAppend = false;
                    i++; // Skip the next part (the filename)
                }
            }
            else if (part == "2>")
            {
                // Stderr redirection (overwrite)
                if (i + 1 < parts.Count)
                {
                    redirection.StderrFile = parts[i + 1];
                    redirection.StderrAppend = false;
                    i++; // Skip the next part (the filename)
                }
            }
            else if (part == ">>" || part == "1>>")
            {
                // Stdout redirection (append)
                if (i + 1 < parts.Count)
                {
                    redirection.StdoutFile = parts[i + 1];
                    redirection.StdoutAppend = true;
                    i++; // Skip the next part (the filename)
                }
            }
            else if (part == "2>>")
            {
                // Stderr redirection (append)
                if (i + 1 < parts.Count)
                {
                    redirection.StderrFile = parts[i + 1];
                    redirection.StderrAppend = true;
                    i++; // Skip the next part (the filename)
                }
            }
            else
            {
                // Regular argument
                filteredParts.Add(part);
            }
        }

        // First part is the command name (can be quoted), rest are arguments
        string commandName = filteredParts[0];
        string[] commandArgs = filteredParts.Skip(1).ToArray();

        return new Command(commandName, commandArgs, redirection);
    }
}

public class Pipeline
{
    public IReadOnlyList<Command> Commands { get; }

    public Pipeline(IEnumerable<Command> commands)
    {
        if (commands == null) throw new ArgumentNullException(nameof(commands));
        Commands = new List<Command>(commands);
    }

    public bool IsSingleCommand => Commands.Count == 1;
}
