
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Diagnostics;
using Serilog;

namespace ShareX.Core;

public static class DebugHelper
{
    // Replace custom Logger with Serilog's static Log class
    public static ILogger Logger { get; private set; }

    public static void Init(string logFilePath)
    {
        var loggerConfig = new LoggerConfiguration()
            // .ReadFrom.Configuration(ShareX.Configuration)
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .WriteTo.Async(a => a.File(logFilePath, rollingInterval: RollingInterval.Day, buffered: true));
        if (ShareX.LogToConsole)
        {
            loggerConfig = loggerConfig.WriteTo.Console();
        }
        Logger = loggerConfig.CreateLogger();
    }

    public static void WriteLine(string message = "")
    {
        if (Logger != null)
        {
            Logger.Information(message); // Log using Serilog Information level
        }
        else
        {
            Debug.WriteLine(message);
        }
    }

    // Write a formatted message
    public static void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args)); // Formatting and passing the result to WriteLine
    }
    // Write an exception message
    public static void WriteException(string exception, string message = "Exception")
    {
        if (Logger != null)
        {
            Logger.Error("{Message} - {Exception}", message, exception); // Log using Serilog Error level
        }
        else
        {
            Console.Error.WriteLine($"{message} - {exception}");
        }
    }

    // Write an exception (serilog will log Exception details)
    public static void WriteException(Exception exception, string message = "Exception")
    {
        WriteException(exception.ToString(), message);
    }

    // This can be omitted if you don't specifically need to flush logs manually
    public static void Flush()
    {
        // Serilog automatically handles flushing, so this method is not strictly needed unless you use async logging.
        // However, you can still use it if you need to force flushing, especially if you have custom sinks.
        Log.CloseAndFlush();
    }
}

