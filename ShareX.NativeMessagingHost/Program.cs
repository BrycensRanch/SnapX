
// SPDX-License-Identifier: GPL-3.0-or-later


using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ShareX.Core.Utils;

if (args.Length == 0)
{
    Console.WriteLine("This executable is used to receive data from a browser addon and send it to ShareX.");
    return;
}

try
{
    var host = new ShareX.Core.CLI.NativeMessagingHost();
    string input = host.Read();

    if (!string.IsNullOrEmpty(input))
    {
        host.Write(input);
        // TODO: This code is no longer correct with ShareX's new structure.
        // Windows: ShareX.Avalonia OR ShareX.CLI
        // macOS: ShareX.Avalonia OR ShareX.CLI
        // Linux: ShareX.GTK4 OR ShareX.Avalonia OR ShareX.CLI
        var filePath = FileHelpers.GetAbsolutePath("ShareX");

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

