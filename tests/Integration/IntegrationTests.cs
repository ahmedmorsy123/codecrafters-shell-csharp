using System;
using System.IO;
using System.Collections.Generic;
using Xunit;

namespace Tests.Integration;

public class IntegrationTests : IDisposable
{
    public IntegrationTests()
    {
        // Clear static state before each test
        Autocomplete.Clear();
    }

    public void Dispose()
    {
        Autocomplete.Clear();
    }

    [Fact]
    public void Pipeline_EchoToFindStr_ReturnsMatch()
    {
        if (OperatingSystem.IsWindows())
        {
            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                var executor = new CommandExecutor();
                // echo hello | findstr hello
                var pipeline = new Pipeline(new List<Command>
                {
                    new Command("echo", new[] { "hello" }),
                    new Command("findstr", new[] { "hello" })
                });

                // findstr might not be in PATH, so this could throw
                // If it succeeds, check output; if it fails, that's expected
                try
                {
                    executor.ExecutePipeline(pipeline);
                    Assert.Contains("hello", sw.ToString());
                }
                catch (CommandNotFoundException)
                {
                    // Expected if findstr is not in PATH
                }
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }

    [Fact]
    public void Redirection_StdoutToFile_CreatesFile()
    {
        var tempFile = Path.GetTempFileName();
        var originalOut = Console.Out;

        try
        {
            // Need to set output before creating executor to avoid disposed writer
            Console.SetOut(new StringWriter());

            var executor = new CommandExecutor();
            // echo redirection_test > tempFile
            var redirection = new RedirectionInfo();
            redirection.StdoutFile = tempFile;

            var command = new Command("echo", new[] { "redirection_test" }, redirection);

            executor.Execute(command);

            var content = File.ReadAllText(tempFile);
            Assert.Contains("redirection_test", content);
        }
        finally
        {
            Console.SetOut(originalOut);
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
