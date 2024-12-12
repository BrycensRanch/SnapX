using System.Reflection;
using ShareX.CLI;


var sharex = new ShareX.Core.ShareX();
sharex.silenceLogging();
sharex.start();
var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
{
    var changelog = new CLIChangelog(version);
    changelog.Display();
}

if (string.Join(" ", args) == "--about" || args[0] == "-v"  || args[0] == "--version" || args[0] == "about")
{
    var about = new CLIAbout();
    about.Show();
}

sharex.shutdown();
