using System;
using System.IO;
using Xunit;

namespace Tests.Commands;

public class DateCommandTests : IDisposable
{
    private readonly StringWriter _output;
    private readonly TextWriter _originalOutput;

    public DateCommandTests()
    {
        _output = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_output);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _output.Dispose();
    }

    [Fact]
    public void Date_WithNoArguments_DisplaysDefaultFormat()
    {
        var cmd = new DateCommand();
        cmd.Execute(Array.Empty<string>());

        var output = _output.ToString().Trim();
        // Should contain day, month, time, and year
        Assert.NotEmpty(output);
        // Check it looks like a date string
        Assert.Matches(@"\w{3}\s+\w{3}\s+\d{2}\s+\d{2}:\d{2}:\d{2}\s+\d{4}", output);
    }

    [Fact]
    public void Date_WithYearFormat_DisplaysYear()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%Y" });

        var output = _output.ToString().Trim();
        Assert.Equal(DateTime.Now.Year.ToString(), output);
    }

    [Fact]
    public void Date_WithMonthFormat_DisplaysMonth()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%m" });

        var output = _output.ToString().Trim();
        var expectedMonth = DateTime.Now.Month.ToString("D2");
        Assert.Equal(expectedMonth, output);
    }

    [Fact]
    public void Date_WithDayFormat_DisplaysDay()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%d" });

        var output = _output.ToString().Trim();
        var expectedDay = DateTime.Now.Day.ToString("D2");
        Assert.Equal(expectedDay, output);
    }

    [Fact]
    public void Date_WithTimeFormat_DisplaysTime()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%H:%M:%S" });

        var output = _output.ToString().Trim();
        // Should be in HH:MM:SS format
        Assert.Matches(@"\d{2}:\d{2}:\d{2}", output);
    }

    [Fact]
    public void Date_WithFullWeekdayName_DisplaysWeekday()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%A" });

        var output = _output.ToString().Trim();
        var expectedDay = DateTime.Now.ToString("dddd");
        Assert.Equal(expectedDay, output);
    }

    [Fact]
    public void Date_WithFullMonthName_DisplaysMonth()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%B" });

        var output = _output.ToString().Trim();
        var expectedMonth = DateTime.Now.ToString("MMMM");
        Assert.Equal(expectedMonth, output);
    }

    [Fact]
    public void Date_WithComplexFormat_DisplaysCorrectly()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%Y-%m-%d" });

        var output = _output.ToString().Trim();
        var expected = DateTime.Now.ToString("yyyy-MM-dd");
        Assert.Equal(expected, output);
    }

    [Fact]
    public void Date_WithTextAndFormats_DisplaysCorrectly()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+Today is %A, %B %d, %Y" });

        var output = _output.ToString().Trim();
        Assert.StartsWith("Today is ", output);
        Assert.Contains(",", output);
    }

    [Fact]
    public void Date_WithEscapedPercent_DisplaysLiteralPercent()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%%Y = %Y" });

        var output = _output.ToString().Trim();
        Assert.Contains("%Y = ", output);
        Assert.Contains(DateTime.Now.Year.ToString(), output);
    }

    [Fact]
    public void Date_WithUnknownFormatSpecifier_PrintsAsIs()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%Z" });

        var output = _output.ToString().Trim();
        // Unknown specifiers should be printed as-is
        Assert.Equal("%Z", output);
    }

    [Fact]
    public void Date_WithMultipleArguments_UsesFirst()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+%Y", "+%m" });

        var output = _output.ToString().Trim();
        Assert.Equal(DateTime.Now.Year.ToString(), output);
    }

    [Fact]
    public void Date_WithoutPlusSign_DisplaysDefaultFormat()
    {
        // If argument doesn't start with +, should display default format
        var cmd = new DateCommand();
        cmd.Execute(new[] { "%Y" });

        var output = _output.ToString().Trim();
        // Should display default format, not interpret as format string
        Assert.NotEmpty(output);
        Assert.Matches(@"\w{3}\s+\w{3}\s+\d{2}\s+\d{2}:\d{2}:\d{2}\s+\d{4}", output);
    }

    [Fact]
    public void Date_ReturnsTrue()
    {
        var cmd = new DateCommand();
        var result = cmd.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Date_WithOnlyPlus_DisplaysPlus()
    {
        var cmd = new DateCommand();
        cmd.Execute(new[] { "+" });

        var output = _output.ToString().Trim();
        Assert.Equal(string.Empty, output);
    }
}
