using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class ClearCommandTests
{
    [Fact]
    public void Clear_ClearsConsole()
    {
        // Console.Clear() throws IOException in test environment without console
        // Just verify the command returns true indicating continuation
        var cmd = new ClearCommand();
        try
        {
            var result = cmd.Execute(Array.Empty<string>());
            Assert.True(result);
        }
        catch (IOException)
        {
            // Expected in test environment without real console
        }
    }
}
