using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NerdbankGitVersioning;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Serilog.Log;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter("Output Directory")]
    readonly AbsolutePath OutputDirectory = RootDirectory / "Output";
    const string Namespace = "SnapX.";

    static string[] ProjectNames = new[] { "GTK4", "Avalonia", "CLI", "NativeMessagingHost" };
    readonly string[] ProjectsToBuild = ProjectNames
        .Where(projectName => OperatingSystem.IsLinux() || projectName != "GTK4")
        .Select(projectName => Path.Combine(RootDirectory, Namespace + projectName, Namespace + projectName + ".csproj"))
        .ToArray();
    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [Parameter("Path to NativeMessagingHost for web extension support")]
    string NMHostPath;
    [Parameter("PREFIX")]
    string Prefix = OperatingSystem.IsWindows() ? "" : Path.Combine(Path.DirectorySeparatorChar.ToString(), "usr", "local");
    // When I used MAKEFILES on Windows, I was using MSYS2 that gave me an acceptable UNIX like path
    // Now, I have no idea what to default to on Windows. Good luck.
    [Parameter("DESTDIR")]
    string Destdir = OperatingSystem.IsWindows() ? "" : "";

    [Parameter("TARGET")] string Target = "snapx-ui";

    [NerdbankGitVersioning]
    readonly NerdbankGitVersioning NerdbankVersioning;


    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {

            DotNetClean();
            OutputDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            foreach (var project in ProjectsToBuild)
            {
                DotNetRestore(s => s
                    .SetProjectFile(project));
            }

        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var projectsThatWereBuilt = new Collection<Project>();
            // Build each project and copy the artifacts to Output
            foreach (var project in ProjectsToBuild)
            {
                var projectName = Path.GetFileNameWithoutExtension(project);
                var projectData = Solution.GetProject(projectName);
                projectsThatWereBuilt.Add(projectData);
                var assemblyName = projectData.GetProperty("AssemblyName")!;
                var projectOutput = Path.Combine(OutputDirectory, assemblyName);

                Information($"Publishing {project}");
                DotNetPublish(s => s
                    .SetProject(project)
                    .SetConfiguration(Configuration)
                    .SetOutput(projectOutput)
                    .SetAssemblyVersion(NerdbankVersioning.AssemblyInformationalVersion)
                    .EnableNoLogo()
                    .EnableNoRestore());
                Information($"Artifacts for {projectName} output to {OutputDirectory}");
            }

            if (ProjectsToBuild.Any(projectName => projectName.Contains("NativeMessagingHost")))
            {
                var NMH = Solution.SnapX_NativeMessagingHost;

                var assemblyName = NMH.GetProperty("AssemblyName")!;

                var projectOutput = Path.Combine(OutputDirectory, assemblyName);

                var exeProjects = projectsThatWereBuilt
                    .Where(p => p.GetOutputType() == "Exe" && !p.Name.Contains("NativeMessagingHost"));

                foreach (var exeProject in exeProjects)
                {
                    var exeFileName = exeProject.GetProperty("AssemblyName")!;
                    if (OperatingSystem.IsWindows())
                    {
                        exeFileName += ".exe";
                    }
                    var exeOutputDirectory = Path.Combine(OutputDirectory, exeFileName, assemblyName);

                    File.Copy(Path.Combine(projectOutput, assemblyName), exeOutputDirectory, overwrite: true);
                }

                if (Directory.Exists(projectOutput))
                {
                    Directory.Delete(projectOutput, recursive: true);
                }
            }
            var manifestFiles = Directory.GetFiles(OutputDirectory, "host-manifest-*.json", SearchOption.AllDirectories);
            if (string.IsNullOrWhiteSpace(NMHostPath))
            {
                if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
                {
                    NMHostPath = "/usr/lib/snapx/SnapX_NativeMessagingHost";
                }
                else if (OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
                {
                    NMHostPath = "/usr/local/lib/snapx/SnapX_NativeMessagingHost";
                }
                // On Windows, do not modify the path, so we do nothing since the resource file targets Windows by default.
                // I know there's more operating systems. I'm just going to pretend like they don't exis-
            }
            foreach (var manifestFile in manifestFiles)
            {
                // Hold the brakes, is that Newtonsoft.JSON in disguise?!?!
                var json = JObject.Parse(File.ReadAllText(manifestFile));
                if (string.IsNullOrWhiteSpace(NMHostPath)) continue;

                json["path"] = NMHostPath;

                File.WriteAllText(manifestFile, json.ToString());
            }
        });
    Target Install => _ => _
        .After(Compile)
        .Executes(() =>
        {
        // To be implemented
        });
}
