// SPDX-License-Identifier: GPL-3.0-or-later


using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace SnapX.Core;

public static class DebugHelper
{
    public static ILogger Logger { get; private set; }
    private static List<string> messageBuffer = new();

    public static void Init(string logFilePath)
    {
        if (string.IsNullOrEmpty(logFilePath)) return;
        var loggerConfig = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
            .WriteTo.Debug()
#endif
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Async(a => a.File(logFilePath, rollingInterval: RollingInterval.Day, buffered: true, restrictedToMinimumLevel: LogEventLevel.Information));
        if (SnapX.LogToConsole)
        {
            loggerConfig = loggerConfig.WriteTo.Console(theme: AnsiConsoleTheme.Sixteen);
        }

        if (SnapX.Configuration != null)
        {
            loggerConfig.ReadFrom.Configuration(SnapX.Configuration);
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
            if (Logger is null && SnapX.LogToConsole)
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
            Logger.Error(exception);
        }
        else
        {
            Console.Error.WriteLine($"{message} - {exception}");
        }
    }

    public static void WriteException(Exception exception, string message = "Exception")
    {
        if (!FeatureFlags.DisableTelemetry) SentrySdk.CaptureException(exception);
        WriteException(exception.ToString(), message);
    }
}

