using System.Text.Json;
using System.Text.Json.Serialization;
using SnapX.CommonUI.Types;
using SnapX.Core;
using SnapX.Core.Utils.Extensions;
using SnapX.Core.Utils.Miscellaneous;

namespace SnapX.CommonUI;

[JsonSerializable(typeof(Release))]
[JsonSerializable(typeof(List<Release>))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(List<Tag>))]
[JsonSerializable(typeof(Commit))]
[JsonSerializable(typeof(List<Commit>))]
[JsonSerializable(typeof(GHA))]
[JsonSerializable(typeof(Root))]
internal partial class ChangelogContext : JsonSerializerContext;

// TODO: Triage why Changelog component is broken
public abstract class Changelog
{
    private static readonly HttpClient Client = HttpClientFactory.Get();
    public string Version { get; init; }
    public Version versionSemver;
    public int major;
    public int minor;
    public int patch;

    public JsonSerializerOptions Options = new()
    {
        TypeInfoResolver = ChangelogContext.Default
    };
    public Changelog(string version)
    {
        Version = version;
        versionSemver = new Version(version);
        major = versionSemver.Major;
        minor = versionSemver.Minor;
        patch = versionSemver.Build;
    }


    public virtual async Task<string> GetChangeSummary()
    {
        DebugHelper.WriteLine("GetChangeSummary called.");
        var releaseSummary = await GetLatestReleasesSinceVersion();
        if (IsValidChangelog(releaseSummary))
            return releaseSummary;

        DebugHelper.WriteLine("No GitHub release available. Checking tags instead.");

        var tagSummary = await GetTagsSinceVersion();
        if (IsValidChangelog(tagSummary))
            return tagSummary;

        DebugHelper.WriteLine("No GitHub tags available. Checking GHA Builds instead.");

        var actionSummary = await GetBuildSummaryFromActions();
        if (IsValidChangelog(actionSummary))
            return actionSummary;

        DebugHelper.WriteLine("No GHA Builds available. Outputting recent commits instead.");

        return await GetRecentCommits();
    }
    private bool IsValidChangelog(string changelog)
    {
        DebugHelper.WriteLine($"Validating changelog: {changelog}");
        return !string.IsNullOrWhiteSpace(changelog) && changelog.Length > 4;
    }

    private async Task<string> GetLatestReleasesSinceVersion()
    {
        var response = await Client.GetAsync(Links.ApiGitHub + "/releases");
        if (!response.IsSuccessStatusCode)
            return string.Empty;
        var releases = JsonSerializer.Deserialize<List<Release>>(await response.Content.ReadAsStringAsync(), Options);
        if (releases is null || releases.Count == 0)
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

        return releaseNotes.Count != 0 ? string.Join("\n", releaseNotes) : string.Empty;
    }

    // Helper method to compare versions
    private static bool IsNewerVersion(int releaseMajor, int releaseMinor, int releasePatch, int major, int minor, int patch)
    {
        var releaseVersion = new Version(releaseMajor, releaseMinor, releasePatch);
        var currentVersion = new Version(major, minor, patch);

        return releaseVersion.CompareTo(currentVersion) > 0;
    }

    private async Task<string> GetTagsSinceVersion()
    {
        var response = await Client.GetAsync(Links.ApiGitHub + "/tags");
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var tags = JsonSerializer.Deserialize<List<Tag>>(await response.Content.ReadAsStringAsync(), Options);
        if (tags is null || tags.Count == 0) return string.Empty;

        var tagSummaries = tags
            .Where(tag =>
            {
                var tagParts = tag.Name.Split('.');
                return tagParts.Length > 3 &&
                       int.TryParse(tagParts[3], out var tagBuildNumber) &&
                       tagBuildNumber > patch;
            })
            .Select(tag => $"Tag: {tag.Name} - {tag.Commit.Message}")
            .ToList();

        return tagSummaries.Count != 0 ? string.Join("\n", tagSummaries) : string.Empty;
    }

    private async Task<string> GetBuildSummaryFromActions()
    {
        var response = await Client.GetAsync(Links.ApiGitHub + "/actions/runs");
        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var actions = JsonSerializer.Deserialize<GHA>(await response.Content.ReadAsStringAsync(), Options);
        if ( actions is null || actions.WorkflowRuns.Count == 0)
            return string.Empty;

        var buildSummaries = actions.WorkflowRuns
            .Where(run => run.RunNumber > patch)
            .Select(run => $"Build #{run.RunNumber}:  {run.DisplayTitle} - {run.Status}")
            .ToList();

        return buildSummaries.Count != 0 ? string.Join("\n", buildSummaries) : string.Empty;
    }


    private async Task<string> GetRecentCommits()
    {
        var response = await Client.GetAsync(Links.ApiGitHub + "/commits?per_page=10");
        if (!response.IsSuccessStatusCode)
            return "No commit history available.";

        var commits = JsonSerializer.Deserialize<List<Root>>(await response.Content.ReadAsStringAsync(), Options);
        if (commits?.Any() != true)
            return "No commit history available.";

        var commitMessages = string.Join("\n", commits.Select(commit =>
            $"- {commit.Commit.Message} by {commit.Commit.Author.Name}"));

        return commitMessages;
    }
    // Must be implemented by child classes to display the changelog ie (SnapX.GTK4)
    public abstract void Display();
}
