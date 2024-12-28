using System;
using System.IO;
using System.Linq;
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

    readonly AbsolutePath OutputDirectory = RootDirectory / "Output";
    const string Namespace = "SnapX.";

    static string[] ProjectNames = new[] { "GTK4", "Avalonia", "CLI", "NativeMessagingHost" };
    readonly string[] ProjectsToBuild = ProjectNames
        .Where(projectName => OperatingSystem.IsLinux() || projectName != "GTK4")
        .Select(projectName => Path.Combine(RootDirectory, Namespace + projectName, Namespace + projectName + ".csproj"))
        .ToArray();
    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

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
            }

            DotNetRestore();
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            // Build each project and copy the artifacts to Output
            foreach (var project in ProjectsToBuild)
            {
                Information($"Publishing {project}");
                DotNetPublish(s => s
                    .SetProject(project)
                    .SetConfiguration(Configuration)
                    .SetOutput(OutputDirectory)
                    .SetAssemblyVersion(NerdbankVersioning.AssemblyInformationalVersion)
                    .EnableNoLogo()
                    .EnableNoRestore());

                Information($"Artifacts for {Path.GetFileNameWithoutExtension(project)} output to {OutputDirectory}");
            }
            var hostManifestChrome = OutputDirectory / "Resources" / "host-manifest-chrome.json";
            var hostManifestFirefox = OutputDirectory / "Resources" / "host-manifest-firefox.json";
            // Ok. My job is to change the paths inside the JSON depending on OS or user input.
            // Ignoring Windows as it has a valid relative path

        });

}
