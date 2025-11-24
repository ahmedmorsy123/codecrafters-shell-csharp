using System;

public static class ConsoleHelper
{
    /// <summary>
    /// Moves the cursor left by the specified number of steps (default 1).
    /// </summary>
    public static void MoveCursorLeft(int steps = 1)
    {
        for (int i = 0; i < steps && Console.CursorLeft > 0; i++)
        {
            Console.Write('\b');
        }
    }

    /// <summary>
    /// Clears the current line in the console.
    /// </summary>
    public static void ClearLine(string currentText, int cursorPosition)
    {
        // Move cursor to the beginning of the line
        Console.Write(new string('\b', cursorPosition));
        // Overwrite with spaces
        Console.Write(new string(' ', currentText.Length));
        // Return cursor to start
        Console.Write(new string('\b', currentText.Length));
    }

    /// <summary>
    /// Redraws the text from a start index to the end and positions the cursor correctly.
    /// </summary>
    public static void RedrawFromCursor(string text, int startIndex)
    {
        string suffix = text.Substring(startIndex);
        Console.Write(suffix);
        // Move cursor back to original position
        Console.Write(new string('\b', suffix.Length));
    }

    /// <summary>
    /// Writes a string at the current cursor position.
    /// </summary>
    public static void WriteAtCursor(string value)
    {
        Console.Write(value);
    }
}
