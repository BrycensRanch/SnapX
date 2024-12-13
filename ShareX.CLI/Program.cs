using System.Reflection;
using ShareX.CLI;
using ShareX.Core;


var sharex = new ShareX.Core.ShareX();
sharex.silenceLogging();
sharex.start(args);
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

var sigintReceived = false;


Console.CancelKeyPress += (_, ea) =>
{
    ea.Cancel = true;

    Console.WriteLine("Received SIGINT (Ctrl+C)");
    sigintReceived = true;
    sharex.shutdown();
    Environment.Exit(0);
};
AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    if (!sigintReceived)
    {
        Console.WriteLine("Received SIGTERM");
        sharex.shutdown();
        Environment.Exit(0);
    }
    else
    {
        Console.WriteLine("Received SIGTERM, ignoring it because already processed SIGINT");
    }
};

sharex.shutdown();
