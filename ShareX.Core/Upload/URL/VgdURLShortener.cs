
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.URL;

public class VgdURLShortenerService : URLShortenerService
{
    public override UrlShortenerType EnumValue => UrlShortenerType.VGD;

    public override bool CheckConfig(UploadersConfig config) => true;

    public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new VgdURLShortener();
    }
}

public class VgdURLShortener : IsgdURLShortener
{
    protected override string APIURL => "https://v.gd/create.php";
}

