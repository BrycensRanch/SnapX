#pragma warning disable CA1416 // I know what I'm doing. Windows registry is NOT called on Unix.
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

namespace ShareX.Core.Utils;

public class OsInfo
{
    public static string GetFancyOSNameAndVersion()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsVersion();

        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxVersion();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetmacOSVersion();
        }
        else
        {
            return $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
        }
    }

    static string GetWindowsVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (key == null) return $"Windows {Environment.OSVersion.Version}";
            var productName = key.GetValue("ProductName")?.ToString() ?? "Unknown Windows";
            var releaseId = key.GetValue("ReleaseId")?.ToString() ?? "Unknown Release";
            var currentVersion = key.GetValue("CurrentVersion")?.ToString() ?? "Unknown Version";

            return $"{productName} {releaseId} {currentVersion}";

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Windows version, hmm. {ex.Message}");
            return $"Windows {Environment.OSVersion.Version}";
        }
    }

    static string GetLinuxVersion()
    {
        try
        {
            var osReleaseFile = "/etc/os-release";
            if (File.Exists(osReleaseFile))
            {
                var lines = File.ReadAllLines(osReleaseFile);

                var prettyName = lines.FirstOrDefault(line => line.StartsWith("PRETTY_NAME"))?.Split('=')[1]?.Trim('"');

                if (string.IsNullOrEmpty(prettyName))
                {
                    prettyName = lines.FirstOrDefault(line => line.StartsWith("NAME"))?.Split('=')[1]?.Trim('"');
                    if (string.IsNullOrEmpty(prettyName))
                    {
                        return $"Linux {Environment.OSVersion.Version}";
                    }

                    return prettyName + " " + lines.FirstOrDefault(line => line.StartsWith("VERSION"))?.Split('=')[1]?.Trim('"');
                }

                return prettyName;
            }

            return $"Linux {Environment.OSVersion.Version}";
        }
        catch
        {
            return $"Linux {Environment.OSVersion.Version}";
        }
    }


    static string GetmacOSVersion()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "sw_vers",
                Arguments = "-productName",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(processStartInfo);
            if (process == null) throw new NullReferenceException("Process was null");
            var osName = process.StandardOutput.ReadLine().Trim();
            process.WaitForExit();

            processStartInfo.Arguments = "-productVersion";
            process = Process.Start(processStartInfo);
            var version = process.StandardOutput.ReadLine().Trim();
            process.WaitForExit();

            return $"{osName} {version}";
        }
        catch
        {
            return $"macOS {Environment.OSVersion.Version}";
        }
    }
}
