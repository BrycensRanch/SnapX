using System.Reflection;
using System.Resources;

namespace SnapX.Core.Utils;

public static class Lang
{
    public static readonly ResourceManager ResourceManager = new("SnapX.Core.Localization.Resources", Assembly.GetExecutingAssembly());
    public static string Get(string key) => ResourceManager.GetString(key) ?? key;
    public static string UnhandledException => Get("UnhandledException");
    public static string WelcomeMessage => Get("WelcomeMessage");
    public static string AboutSnapX => Get("AboutSnapX");
    public static string UploadToAmazonS3Failed => Get("UploadToAmazonS3Failed");
    public static string SnapXFailedToStart => Get("SnapXFailedToStart");
    public static string ReportErrorToSentry => Get("ReportErrorToSentry");
    public static string CreateGitHubIssue => Get("CreateGitHubIssue");
    public static string CopyErrorToClipboard => Get("CopyErrorToClipboard");
    public static string EditWithSnapX => Get("EditWithSnapX");
    public static string UploadWithSnapX => Get("UploadWithSnapX");
    public static string UploadManagerUploadFile => Get("UploadManagerUploadFile");


}
