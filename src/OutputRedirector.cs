/// <summary>
/// Temporarily redirects Console output and error streams to files or keeps them as-is
/// </summary>
public class OutputRedirector : IDisposable
{
    private readonly TextWriter _originalOut;
    private readonly TextWriter _originalError;
    private StreamWriter? _stdoutWriter;
    private StreamWriter? _stderrWriter;

    public OutputRedirector(RedirectionInfo redirection)
    {
        _originalOut = Console.Out;
        _originalError = Console.Error;

        // Redirect stdout if needed
        if (redirection.HasStdoutRedirection)
        {
            _stdoutWriter = new StreamWriter(redirection.StdoutFile!, redirection.StdoutAppend);
            Console.SetOut(_stdoutWriter);
        }

        // Redirect stderr if needed
        if (redirection.HasStderrRedirection)
        {
            _stderrWriter = new StreamWriter(redirection.StderrFile!, redirection.StderrAppend);
            Console.SetError(_stderrWriter);
        }
    }

    public void Dispose()
    {
        // Restore original streams
        Console.SetOut(_originalOut);
        Console.SetError(_originalError);

        // Close file writers
        _stdoutWriter?.Dispose();
        _stderrWriter?.Dispose();
    }
}
