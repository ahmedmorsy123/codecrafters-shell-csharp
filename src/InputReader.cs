using System.Text;

/// <summary>
/// Handles interactive input with tab autocomplete for builtin commands
/// </summary>
public class InputReader
{
    /// <summary>
    /// Reads a line of input with tab autocomplete support for builtin commands
    /// </summary>
    public static string? ReadLine()
    {
        // Check if stdin is redirected (e.g., from a pipe or file)
        if (Console.IsInputRedirected)
        {
            return ReadRedirectedInput();
        }
        else
        {
            return ReadInteractiveInput();
        }
    }

    /// <summary>
    /// Reads input from redirected stdin (for automated tests)
    /// </summary>
    private static string? ReadRedirectedInput()
    {
        StringBuilder line = new StringBuilder();
        int ch;
        
        while ((ch = Console.Read()) != -1 && ch != '\n' && ch != '\r')
        {
            char c = (char)ch;

            if (c == '\t')  // Tab character
            {
                string? completion = Autocomplete.GetSuggestion(line.ToString());
                if (completion != null && completion != line.ToString())
                {
                    string addedPart = completion.Substring(line.Length);
                    line.Append(addedPart);
                    Console.Write(addedPart);
                }
            }
            else
            {
                line.Append(c);
                Console.Write(c);
            }
        }
        
        // If we reached EOF before reading any characters, signal EOF
        if (ch == -1 && line.Length == 0)
        {
            return null;
        }

        // Handle \r\n (Windows) vs \n (Unix)
        if (ch == '\r')
        {
            int next = Console.Read();
            // Just consume the \n if it exists
        }

        return line.ToString();
    }

    /// <summary>
    /// Reads input interactively with simple Tab completion
    /// </summary>
    private static string ReadInteractiveInput()
    {
        StringBuilder line = new StringBuilder();

        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return line.ToString();
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                // Try to find a completion and append only the missing letters
                string? suggestion = Autocomplete.GetSuggestion(line.ToString());

                if (suggestion != null && suggestion != line.ToString())
                {
                    string addedPart = suggestion.Substring(line.Length);
                    line.Append(addedPart);
                    Console.Write(addedPart);
                }
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                // Append regular characters
                line.Append(keyInfo.KeyChar);
                Console.Write(keyInfo.KeyChar);
            }
            // Ignore all other keys (backspace, delete, arrows, etc.)
        }
    }
}
