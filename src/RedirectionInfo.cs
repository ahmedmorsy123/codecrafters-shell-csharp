/// <summary>
/// Holds information about output/error redirection
/// </summary>
public class RedirectionInfo
{
    public string? StdoutFile { get; set; }
    public bool StdoutAppend { get; set; }
    
    public string? StderrFile { get; set; }
    public bool StderrAppend { get; set; }

    public bool HasStdoutRedirection => StdoutFile != null;
    public bool HasStderrRedirection => StderrFile != null;
}
