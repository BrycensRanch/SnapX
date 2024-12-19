
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Task;

public class RecentTask
{
    public string FilePath { get; set; }

    public string FileName
    {
        get
        {
            string text = "";

            if (!string.IsNullOrEmpty(FilePath))
            {
                text = FilePath;
            }
            else if (!string.IsNullOrEmpty(URL))
            {
                text = URL;
            }

            return FileHelpers.GetFileNameSafe(text);
        }
    }

    public string URL { get; set; }
    public string ThumbnailURL { get; set; }
    public string DeletionURL { get; set; }
    public string ShortenedURL { get; set; }

    public DateTime Time { get; set; }

    public string TrayMenuText
    {
        get
        {
            string text = ToString().Truncate(50, "...", false);

            return string.Format("[{0:HH:mm:ss}] {1}", Time, text);
        }
    }

    public RecentTask()
    {
        Time = DateTime.Now;
    }

    public override string ToString()
    {
        string text = "";

        if (!string.IsNullOrEmpty(ShortenedURL))
        {
            text = ShortenedURL;
        }
        else if (!string.IsNullOrEmpty(URL))
        {
            text = URL;
        }
        else if (!string.IsNullOrEmpty(FilePath))
        {
            text = FilePath;
        }

        return text;
    }
}

