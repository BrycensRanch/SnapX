using ShareX.Core;

namespace ShareX.CLI;
public class CLIChangelog : CommonUI.Changelog
{
    public CLIChangelog(string version) : base(version)
    {
        Version = version;
    }

    public override async void Display()
    {
        // Display changelog in the CLI
        Console.WriteLine($"Changelog for {Version}:");
        var changes = await base.GetChangeSummary();
        Console.WriteLine(changes);
    }
}
