using System.Text.Json;
using SnapX.CommonUI.Types;
using SnapX.Core;
using SnapX.Core.Utils.Miscellaneous;

namespace SnapX.CommonUI;

// TODO: Triage why Changelog component is broken
public abstract class Changelog
{
    private static readonly HttpClient Client = new()
    {
        BaseAddress = new Uri(Links.GitHub),
        Timeout = TimeSpan.FromSeconds(5)
    };
    public string Version { get; init; }
    public Changelog(string version)
    {
        Version = version;
    }


    public virtual async Task<string> GetChangeSummary()
    {
        DebugHelper.WriteLine("GetChangeSummary called.");
        var releaseSummary = await GetLatestReleasesSinceVersion();
        if (!IsValidChangelog(releaseSummary))
            return releaseSummary;
        DebugHelper.WriteLine("No GitHub release available. Checking tags instead.");
        //
        var tagSummary = await GetTagsSinceVersion();
        if (!IsValidChangelog(tagSummary))
            return tagSummary;
        DebugHelper.WriteLine("No GitHub tags available. Checking GHA Builds instead.");

        var actionSummary = await GetBuildSummaryFromActions();
        if (!IsValidChangelog(actionSummary))
            return actionSummary;
        DebugHelper.WriteLine("No GHA Builds available. Outputting recent commits instead.");

        return await GetRecentCommits();
    }
    private bool IsValidChangelog(string changelog)
    {
        DebugHelper.WriteLine($"Validating changelog: {changelog}");
        return !string.IsNullOrWhiteSpace(changelog) && changelog.Any(char.IsLetter) && changelog.Length > 4;
    }

    private async Task<string> GetLatestReleasesSinceVersion()
    {
        var versionParts = Version.Split('.');

        if (versionParts.Length < 3)
            return string.Empty;

        var (major, minor, patch) = (
            int.Parse(versionParts[0]),
            int.Parse(versionParts[1]),
            int.Parse(versionParts[2])
        );

        var response = await Client.GetAsync("/releases");
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var releases = JsonSerializer.Deserialize<List<Release>>(await response.Content.ReadAsStringAsync());
        if (releases?.Count == 0)
            return string.Empty;

        var releaseNotes = releases
            .Where(release =>
            {
                var releaseVersionParts = release.TagName.TrimStart('v').Split('.');
                if (releaseVersionParts.Length < 3) return false;

                var (releaseMajor, releaseMinor, releasePatch) = (
                    int.Parse(releaseVersionParts[0]),
                    int.Parse(releaseVersionParts[1]),
                    int.Parse(releaseVersionParts[2])
                );

                return IsNewerVersion(releaseMajor, releaseMinor, releasePatch, major, minor, patch);
            })
            .Select(release => release.Body)
            .ToList();

        return releaseNotes.Any() ? string.Join("\n", releaseNotes) : string.Empty;
    }

    // Helper method to compare versions
    private bool IsNewerVersion(int releaseMajor, int releaseMinor, int releasePatch, int major, int minor, int patch)
    {
        if (releaseMajor > major) return true;
        if (releaseMajor == major && releaseMinor > minor) return true;
        if (releaseMajor == major && releaseMinor == minor && releasePatch > patch) return true;
        return false;
    }

    private async Task<string> GetTagsSinceVersion()
    {
        var versionParts = Version.Split('.');
        var versionBuildNumber = versionParts.Length > 3 ? versionParts[3] : null;

        if (string.IsNullOrEmpty(versionBuildNumber))
            return string.Empty;

        var response = await Client.GetAsync("/tags");
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var tags = JsonSerializer.Deserialize<List<Tag>>(await response.Content.ReadAsStringAsync());
        if (tags?.Count == 0) return string.Empty;

        var tagSummaries = tags
            .Where(tag =>
            {
                var tagParts = tag.Name.Split('.');
                return tagParts.Length > 3 &&
                       int.TryParse(tagParts[3], out var tagBuildNumber) &&
                       int.TryParse(versionBuildNumber, out var currentVersionBuild) &&
                       tagBuildNumber > currentVersionBuild;
            })
            .Select(tag => $"Tag: {tag.Name} - {tag.Commit.Message}")
            .ToList();

        return tagSummaries.Count != 0 ? string.Join("\n", tagSummaries) : string.Empty;
    }

    private async Task<string> GetBuildSummaryFromActions()
    {
        var versionParts = Version.Split('.');
        var buildNumber = versionParts.Length > 3 ? versionParts[3] : null;

        if (string.IsNullOrEmpty(buildNumber))
            return string.Empty;

        var response = await Client.GetAsync("/actions/runs");
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var actions = JsonSerializer.Deserialize<GHA>(await response.Content.ReadAsStringAsync());
        if (actions?.WorkflowRuns == null || !actions.WorkflowRuns.Any())
            return string.Empty;

        var buildSummaries = actions.WorkflowRuns
            .Where(run => int.TryParse(buildNumber, out var targetBuildNumber) && run.RunNumber > targetBuildNumber)
            .Select(run => $"Build #{run.RunNumber}:  {run.DisplayTitle} - {run.Status}")
            .ToList();

        return buildSummaries.Count != 0 ? string.Join("\n", buildSummaries) : string.Empty;
    }


    private async Task<string> GetRecentCommits()
    {
        var response = await Client.GetAsync("/commits?per_page=10");
        if (!response.IsSuccessStatusCode)
            return "No commit history available.";

        var commits = JsonSerializer.Deserialize<List<Root>>(await response.Content.ReadAsStringAsync());
        if (commits?.Any() != true)
            return "No commit history available.";

        var commitMessages = string.Join("\n", commits.Select(commit =>
            $"- {commit.Commit.Message} by {commit.Commit.Author.Name}"));

        return commitMessages;
    }
    // Must be implemented by child classes to display the changelog ie (SnapX.GTK4)
    public abstract void Display();
}
