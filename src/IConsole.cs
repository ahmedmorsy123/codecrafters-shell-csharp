using System;

public interface IConsole
{
    ConsoleKeyInfo ReadKey(bool intercept);
    int Read();
    void Write(string value);
    void WriteLine(string value);
    void Write(char value);
    bool IsInputRedirected { get; }
    int CursorLeft { get; set; }
    int BufferWidth { get; }
}
