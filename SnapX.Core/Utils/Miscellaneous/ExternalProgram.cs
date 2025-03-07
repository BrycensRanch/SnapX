
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Diagnostics;

namespace SnapX.Core.Utils.Miscellaneous;

public class ExternalProgram
{
    public bool IsActive { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Args { get; set; }
    public string OutputExtension { get; set; }
    public string Extensions { get; set; }
    public bool HiddenWindow { get; set; }
    public bool DeleteInputFile { get; set; }

    private string pendingInputFilePath;
    private string userInput { get; set; }

    public ExternalProgram()
    {
        Args = '"' + userInput + '"';
    }

    public ExternalProgram(string name, string path) : this()
    {
        Name = name;
        Path = path;
    }

    public string GetFullPath()
    {
        return FileHelpers.ExpandFolderVariables(Path);
    }

    public string Run(string inputPath)
    {
        pendingInputFilePath = null;
        string path = GetFullPath();

        if (!string.IsNullOrEmpty(path) && File.Exists(path) && !string.IsNullOrWhiteSpace(inputPath))
        {
            inputPath = inputPath.Trim('"');

            if (CheckExtension(inputPath))
            {
                try
                {
                    var outputPath = inputPath;

                    string arguments;

                    if (string.IsNullOrEmpty(Args))
                    {
                        arguments = '"' + inputPath + '"';
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(OutputExtension))
                        {
                            outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(inputPath), System.IO.Path.GetFileNameWithoutExtension(inputPath));

                            if (!OutputExtension.StartsWith("."))
                            {
                                outputPath += ".";
                            }

                            outputPath += OutputExtension;
                        }

                        arguments = "";
                    }

                    using (Process process = new Process())
                    {
                        ProcessStartInfo psi = new ProcessStartInfo()
                        {
                            FileName = path,
                            Arguments = arguments,
                            UseShellExecute = false,
                            CreateNoWindow = HiddenWindow
                        };

                        DebugHelper.WriteLine($"Action input: \"{inputPath}\" [{FileHelpers.GetFileSizeReadable(inputPath)}]");
                        DebugHelper.WriteLine($"Action run: \"{psi.FileName}\" {psi.Arguments}");

                        process.StartInfo = psi;
                        process.Start();
                        process.WaitForExit();
                    }

                    if (!string.IsNullOrEmpty(outputPath) && File.Exists(outputPath))
                    {
                        DebugHelper.WriteLine($"Action output: \"{outputPath}\" [{FileHelpers.GetFileSizeReadable(outputPath)}]");

                        if (DeleteInputFile && !inputPath.Equals(outputPath, StringComparison.OrdinalIgnoreCase))
                        {
                            pendingInputFilePath = inputPath;
                        }

                        return outputPath;
                    }

                    return inputPath;
                }
                catch (Exception e)
                {
                    DebugHelper.WriteException(e);
                }
            }
        }

        return null;
    }

    public Task<string> RunAsync(string inputPath)
    {
        return Task.Run(() => Run(inputPath));
    }

    public bool CheckExtension(string path)
    {
        return CheckExtension(path, Extensions);
    }

    public bool CheckExtension(string path, string extensions)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            if (string.IsNullOrWhiteSpace(extensions))
            {
                return true;
            }

            int index = 0;

            for (int i = 0; i <= extensions.Length; ++i)
            {
                if (i == extensions.Length || !char.IsLetterOrDigit(extensions[i]))
                {
                    if (i > index)
                    {
                        string extension = "." + extensions.Substring(index, i - index);

                        if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }

                    index = i + 1;
                }
            }
        }

        return false;
    }

    public void DeletePendingInputFile()
    {
        string inputPath = pendingInputFilePath;

        if (!string.IsNullOrEmpty(inputPath) && File.Exists(inputPath))
        {
            DebugHelper.WriteLine($"Deleting input file: \"{inputPath}\"");

            try
            {
                File.Delete(inputPath);
                pendingInputFilePath = null;
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }
    }
}

