#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using ShareX.Core;

namespace ShareX.NativeMessagingHost
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    var host = new Core.NativeMessagingHost();
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
                        if (process.ExitCode == 0) return;
                        Console.Error.WriteLine($"Process exited with error code {process.ExitCode}");
                        Console.Error.WriteLine($"Error output: {error}");
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("This executable is used to receive data from a browser addon and send it to ShareX.");
            }
        }
    }
}
