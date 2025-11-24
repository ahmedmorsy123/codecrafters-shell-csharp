using System;
using Xunit;

namespace Tests.Core;

public class RealConsoleTests
{
    [Fact]
    public void RealConsole_ImplementsIConsole()
    {
        var console = new RealConsole();
        Assert.IsAssignableFrom<IConsole>(console);
    }

    [Fact]
    public void RealConsole_IsInputRedirected_ReturnsConsoleValue()
    {
        var console = new RealConsole();
        var result = console.IsInputRedirected;
        // Should match Console.IsInputRedirected
        Assert.Equal(Console.IsInputRedirected, result);
    }

    [Fact]
    public void RealConsole_BufferWidth_ReturnsValue()
    {
        var console = new RealConsole();
        try
        {
            var width = console.BufferWidth;
            Assert.True(width > 0);
        }
        catch (System.IO.IOException)
        {
            // Expected in test environment without console
            Assert.True(true);
        }
    }

    [Fact]
    public void RealConsole_Write_String_DoesNotThrow()
    {
        var console = new RealConsole();
        var exception = Record.Exception(() => console.Write("test"));
        Assert.Null(exception);
    }

    [Fact]
    public void RealConsole_Write_Char_DoesNotThrow()
    {
        var console = new RealConsole();
        var exception = Record.Exception(() => console.Write('x'));
        Assert.Null(exception);
    }

    [Fact]
    public void RealConsole_WriteLine_DoesNotThrow()
    {
        var console = new RealConsole();
        var exception = Record.Exception(() => console.WriteLine("test"));
        Assert.Null(exception);
    }

    [Fact]
    public void RealConsole_CursorLeft_CanBeAccessed()
    {
        var console = new RealConsole();
        try
        {
            var position = console.CursorLeft;
            Assert.True(position >= 0);
        }
        catch (System.IO.IOException)
        {
            // Expected in test environment without console
            Assert.True(true);
        }
    }

    [Fact]
    public void RealConsole_CursorLeft_CanBeSet()
    {
        var console = new RealConsole();
        try
        {
            var original = console.CursorLeft;
            console.CursorLeft = original;
            Assert.Equal(original, console.CursorLeft);
        }
        catch (System.IO.IOException)
        {
            // Expected in test environment without console
            Assert.True(true);
        }
    }
}
