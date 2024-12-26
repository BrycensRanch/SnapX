using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace SnapX.Core.Utils;

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
    [SupportedOSPlatform("windows")]
    static string GetWindowsVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            if (key == null)
                return $"Windows {Environment.OSVersion.Version}";

            var productName = key.GetValue("ProductName")?.ToString() ?? "Unknown Windows";
            var releaseId = key.GetValue("ReleaseId")?.ToString() ?? "Unknown Release";
            var currentBuild = key.GetValue("CurrentBuild")?.ToString() ?? "Unknown Version";

            if (Helpers.IsWindows11OrGreater())
                productName = productName.Replace("10", "11");

            return $"{productName} {releaseId} {currentBuild}";

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Windows version, hmm.{Environment.NewLine}{ex.ToString}");
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
