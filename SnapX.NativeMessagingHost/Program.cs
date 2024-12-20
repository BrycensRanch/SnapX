
// SPDX-License-Identifier: GPL-3.0-or-later


using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using SnapX.Core.Utils;

if (args.Length == 0)
{
    Console.WriteLine("This executable is used to receive data from a browser addon and send it to SnapX.");
    return;
}

try
{
    var host = new SnapX.Core.CLI.NativeMessagingHost();
    string input = host.Read();

    if (!string.IsNullOrEmpty(input))
    {
        host.Write(input);
        // TODO: This code is no longer correct with SnapX's new structure.
        // Windows: SnapX.Avalonia OR SnapX.CLI
        // macOS: SnapX.Avalonia OR SnapX.CLI
        // Linux: SnapX.GTK4 OR SnapX.Avalonia OR SnapX.CLI
        var filePath = FileHelpers.GetAbsolutePath("SnapX");

        var tempFilePath = FileHelpers.GetTempFilePath("json");
        File.WriteAllText(tempFilePath, input, Encoding.UTF8);

        var startInfo = new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = $"-NativeMessagingInput \"{tempFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) return;
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Debug.WriteLine($"Output: {output}");
        if (process.ExitCode == 0) return;
        Console.Error.WriteLine($"Process exited with error code {process.ExitCode}");
        Console.Error.WriteLine($"Error output: {error}");
    }
}
catch (Exception e)
{
    Console.Error.WriteLine($"Error: {e.Message}");
}

