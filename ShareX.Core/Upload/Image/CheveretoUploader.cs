
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Utils;

namespace ShareX.Core.Upload.Image;

public class CheveretoUploader
{
    public string UploadURL { get; set; }
    public string APIKey { get; set; }

    public CheveretoUploader()
    {
    }

    public CheveretoUploader(string uploadURL, string apiKey)
    {
        UploadURL = uploadURL;
        APIKey = apiKey;
    }

    public override string ToString()
    {
        return URLHelpers.GetHostName(UploadURL);
    }
}

