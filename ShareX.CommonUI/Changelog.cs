using System.Text.Json;
using System.Text.Json.Nodes;
using ShareX.CommonUI.Types;
using ShareX.Core;

namespace ShareX.CommonUI;

public abstract class Changelog
{
        private static readonly HttpClient Client = new()
        {
            BaseAddress = new Uri("https://api.github.com/repos/BrycensRanch/ShareX-Linux-Port"),
            Timeout = TimeSpan.FromSeconds(5)
        };
        public string Version { get; init; }
        public Changelog(string version)
        {
            Version = version;
        }


        public virtual async Task<string> GetChangeSummary()
        {
            try
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
            catch (Exception ex)
            {
                throw;
                return $"Error retrieving changelog: {ex.Message}";
            }
        }
private bool IsValidChangelog(string changelog) {
    DebugHelper.WriteLine($"Validating changelog: {changelog}");
    return !string.IsNullOrWhiteSpace(changelog) && changelog.Any(char.IsLetter) && changelog.Length > 4;
}
private async Task<string> GetLatestReleasesSinceVersion()
{
    var versionParts = Version.Split('.');
    if (versionParts.Length < 3)
        return string.Empty;  // Return empty if the version format is not valid

    var major = int.Parse(versionParts[0]);
    var minor = int.Parse(versionParts[1]);
    var patch = int.Parse(versionParts[2]);

    var response = await Client.GetAsync("/releases");
    if (!response.IsSuccessStatusCode)
        return string.Empty;

    var json = await response.Content.ReadAsStringAsync();
    var releases = JsonSerializer.Deserialize<List<Release>>(json);
    if (releases?.Count == 0) return string.Empty;

    var releaseNotes = new List<string>();

    foreach (var release in releases)
    {
        var tagName = release.TagName;

        // Parse the version tag, e.g., "v1.2.3" -> "1.2.3"
        var releaseVersionParts = tagName.TrimStart('v').Split('.');
        if (releaseVersionParts.Length < 3)
            continue;  // Skip if the version format is invalid

        var releaseMajor = int.Parse(releaseVersionParts[0]);
        var releaseMinor = int.Parse(releaseVersionParts[1]);
        var releasePatch = int.Parse(releaseVersionParts[2]);

        if (!IsNewerVersion(releaseMajor, releaseMinor, releasePatch, major, minor, patch))
        {
            continue;
        }

        releaseNotes.Add(release.Body);

    }

    return releaseNotes.Count > 0 ? string.Join("\n", releaseNotes) : string.Empty;
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
            var versionNumber = versionParts.Length > 3 ? versionParts[3] : null;
            if (string.IsNullOrEmpty(versionNumber))
                return string.Empty;

            var response = await Client.GetAsync("/tags");
            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var json = await response.Content.ReadAsStringAsync();
            var tags = JsonSerializer.Deserialize<List<Tag>>(json);
            if (tags?.Count == 0) return string.Empty;

            var tagSummaries = new List<string>();

            foreach (var tag in tags)
            {
                // Assuming tag.Name is in a format like "v1.0.1", "v1.1.0", etc.
                var tagParts = tag.Name.Split('.');
                if (tagParts.Length > 3 && int.TryParse(tagParts[3], out var tagBuildNumber) &&
                    int.TryParse(versionNumber, out var versionBuildNumber) &&
                    tagBuildNumber > versionBuildNumber)
                {
                    // Add the tag and its description to the list
                    tagSummaries.Add($"Tag: {tag.Name} - {tag.Commit.Message}");
                }
            }

            return tagSummaries.Count > 0 ? string.Join("\n", tagSummaries) : string.Empty;
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

            var json = await response.Content.ReadAsStringAsync();
            var actions = JsonSerializer.Deserialize<GHA>(json);
            if (actions == null) return string.Empty;


            // List to hold summaries of builds after the specified build number
            var buildSummaries = new List<string>();

            foreach (var run in actions.WorkflowRuns)
            {
                var currentRunNumber = run.RunNumber;

                if (
                    int.TryParse(buildNumber, out var targetBuildNumber) &&
                    currentRunNumber <= targetBuildNumber)
                {
                    continue;
                }

                buildSummaries.Add($"Build #{currentRunNumber}:  {run.DisplayTitle} - {run.Status}");
            }

            return buildSummaries.Count > 0 ? string.Join("\n", buildSummaries) : string.Empty;
        }


        private async Task<string> GetRecentCommits()
        {
            var response = await Client.GetAsync("/commits?per_page=10");
            if (!response.IsSuccessStatusCode)
                return "No commit history available.";

            var json = await response.Content.ReadAsStringAsync();
            DebugHelper.WriteLine(json);
            var commits = JsonSerializer.Deserialize<List<Root>>(json);
            if (commits == null || commits.Count == 0) return  "No commit history available.";
            var commitMessages = "";
            foreach (var commit in commits)
            {
                commitMessages += $"- {commit.Commit.Message} by {commit.Commit.Author.Name}\n";
            }

            return commitMessages.Length > 0 ? commitMessages : "No commit history available.";
        }
        // Must be implemented by child classes to display the changelog ie (ShareX.GTK4)
        public abstract void Display();
}
