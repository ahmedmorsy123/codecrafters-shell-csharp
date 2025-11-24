using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class ExitCommandTests
{
    [Fact]
    public void Exit_ReturnsFalse()
    {
        var cmd = new ExitCommand();
        bool result = cmd.Execute(Array.Empty<string>());
        Assert.False(result);
    }

    [Fact]
    public void Exit_WithArguments_StillReturnsFalse()
    {
        var cmd = new ExitCommand();
        bool result = cmd.Execute(new[] { "0" });
        Assert.False(result);
    }
}
