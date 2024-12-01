using ShareX.CommonUI;

namespace ShareX.CLI;

public class CLIAbout : AboutDialog
{
    public override string GetTitle() => "ShareX CLI";

    public override void Show()
    {
        Console.WriteLine($"===============  {GetTitle()}     =================");
        Console.WriteLine($"{GetDescription()}");
        Console.WriteLine($"Version: {GetVersion()}");
        Console.WriteLine($"{GetCopyright()} Licensed under {GetLicense()}");
        Console.WriteLine($"GitHub: {GetWebsite()}");
        Console.WriteLine($"OS: {GetSystemInfo()} ({GetOsArchitecture()})");
        Console.WriteLine($".NET Version: {GetRuntime()}");
        Console.WriteLine($"Platform: {GetOsPlatform()}");
        Console.WriteLine("===================================================");

    }
}
