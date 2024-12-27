using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SnapX.Core.Utils.Native;

public class MacOSAPI : NativeAPI
{
    // Finally, I can breathe!
    // No need to call DLLImport.
    // It's beautiful.
    public override void CopyText(string text)
    {
        // Escape quotes in the text to ensure AppleScript handles them correctly
        // 1. Escape double quotes by replacing `"` with `""` for AppleScript
        string escapedText = text.Replace("\"", "\"\"");

        // 2. Escape backslashes by replacing `\` with `\\` (for C# string formatting)
        escapedText = "\"" + Regex.Replace(escapedText, @"(\\+)$", @"$1$1") + "\""; ;

        // Properly format the AppleScript to set the clipboard
        var appleScript = $"set the clipboard to \"{escapedText}\"";

        // Create the process to execute the AppleScript
        var process = new Process();
        process.StartInfo.FileName = "osascript";
        process.StartInfo.Arguments = $"-e \"{appleScript}\"";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;

        // Start the process
        process.Start();

        // Wait for the process to finish
        process.WaitForExit();
    }

}
