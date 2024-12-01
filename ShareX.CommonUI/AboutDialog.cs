using System.Reflection;

namespace ShareX.CommonUI;

public class AboutDialog
{
    public virtual void Show()
    {
        throw new NotImplementedException();
    }

    public virtual string GetSystemInfo()
    {
        return Core.Utils.OsInfo.GetFancyOSNameAndVersion();
    }
    public virtual string GetTitle() => "ShareX";
    public virtual string GetLicense() => "GPL v3 or Later";
    public virtual string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    public virtual string GetWebsite() => "https://github.com/BrycensRanch/ShareX-Linux-Port";
    public virtual string GetDescription() => Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "Image sharing tool";
    public virtual string GetCopyright() => "© BrycensRanch & ShareX Team 2024-present";
    public virtual string GetRuntime() => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    public virtual string GetOsPlatform() => $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}";
    public virtual string GetOsArchitecture() => System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();


}
