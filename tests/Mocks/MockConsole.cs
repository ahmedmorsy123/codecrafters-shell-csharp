using System;
using System.Collections.Generic;
using System.Text;

public class MockConsole : IConsole
{
    private readonly Queue<ConsoleKeyInfo> _keyQueue = new Queue<ConsoleKeyInfo>();
    private readonly Queue<int> _readQueue = new Queue<int>();
    private readonly StringBuilder _output = new StringBuilder();
    private readonly StringBuilder _error = new StringBuilder();
    private bool _isInputRedirected = false;

    public bool IsInputRedirected => _isInputRedirected;
    public int CursorLeft { get; set; }
    public int BufferWidth => 120;

    public void SetInputRedirected(bool value) => _isInputRedirected = value;
    public void EnqueueKey(ConsoleKeyInfo keyInfo) => _keyQueue.Enqueue(keyInfo);
    public void EnqueueChar(int ch) => _readQueue.Enqueue(ch);
    public void EnqueueReadChar(int ch) => _readQueue.Enqueue(ch);

    public ConsoleKeyInfo ReadKey(bool intercept) => _keyQueue.Dequeue();
    public int Read() => _readQueue.Count > 0 ? _readQueue.Dequeue() : -1;
    public void Write(string value) => _output.Append(value);
    public void WriteLine(string value) => _output.AppendLine(value);
    public void Write(char value) => _output.Append(value);
    public string GetOutput() => _output.ToString();
    public string GetError() => _error.ToString();
}
