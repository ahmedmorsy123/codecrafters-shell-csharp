using System;

public class RealConsole : IConsole
{
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    public int Read() => Console.Read();
    public void Write(string value) => Console.Write(value);
    public void WriteLine(string value) => Console.WriteLine(value);
    public void Write(char value) => Console.Write(value);
    public bool IsInputRedirected => Console.IsInputRedirected;
    public int CursorLeft
    {
        get => Console.CursorLeft;
        set => Console.CursorLeft = value;
    }
    public int BufferWidth => Console.BufferWidth;
}
