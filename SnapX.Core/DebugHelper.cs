// SPDX-License-Identifier: GPL-3.0-or-later


using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace SnapX.Core;

public static class DebugHelper
{
    public static ILogger Logger { get; private set; }
    private static List<string> messageBuffer = new();

    public static void Init(string logFilePath)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Async(a => a.File(logFilePath, rollingInterval: RollingInterval.Day, buffered: true));
        if (SnapX.LogToConsole)
        {
            loggerConfig = loggerConfig.WriteTo.Console(theme: AnsiConsoleTheme.Sixteen);
        }
        Logger = loggerConfig.CreateLogger();
    }

    public static void WriteLine(string message = "")
    {
        if (Logger != null)
        {
            foreach (var bufferedMessage in messageBuffer)
            {
                Logger.Information(bufferedMessage);
            }
            Logger.Information(message);

            messageBuffer.Clear();
        }
        else
        {
            messageBuffer.Add(message);
        }
    }
    public static void FlushBufferedMessages()
    {
        foreach (var bufferedMessage in messageBuffer)
        {
            if (Logger is null)
            {
                Console.WriteLine(bufferedMessage);
            }
            else
            {
                Logger.Information(bufferedMessage);
            }
        }

        messageBuffer.Clear();
    }
    public static void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }
    public static void WriteException(string exception, string message = "Exception")
    {
        if (Logger != null)
        {
            Logger.Error("{Message} - {Exception}", message, exception);
        }
        else
        {
            Console.Error.WriteLine($"{message} - {exception}");
        }
    }

    public static void WriteException(Exception exception, string message = "Exception")
    {
        WriteException(exception.ToString(), message);
    }
}

