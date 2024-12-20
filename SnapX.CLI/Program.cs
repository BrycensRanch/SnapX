using SnapX.CLI;
using SnapX.Core;
using SnapX.Core.Utils;


var sharex = new SnapX.Core.SnapX();
sharex.silenceLogging();
sharex.start(args);
var version = Helpers.GetApplicationVersion();

if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
{
    var changelog = new CLIChangelog(version);
    changelog.Display();
}

if (string.Join(" ", args) == "--about" || args[0] == "-v" || args[0] == "--version" || args[0] == "about")
{
    var about = new CLIAbout();
    about.Show();
}

var sigintReceived = false;


Console.CancelKeyPress += (_, ea) =>
{
    ea.Cancel = true;
    sigintReceived = true;
    Console.WriteLine("Received SIGINT (Ctrl+C)");
    sharex.shutdown();
    Environment.Exit(0);
};
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    if (!sigintReceived)
    {
        sigintReceived = true;
        DebugHelper.WriteLine("Received SIGTERM");
        sharex.shutdown();
        Environment.Exit(0);
    }
    else
    {
        DebugHelper.WriteLine("Received SIGTERM, ignoring it because already processed SIGINT");
    }
};

sharex.shutdown();
