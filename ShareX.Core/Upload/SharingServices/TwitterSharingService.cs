
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Image;
using ShareX.Core.Upload.OAuth;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Upload.SharingServices;

public class TwitterSharingService : URLSharingService
{
    public override URLSharingServices EnumValue => URLSharingServices.Twitter;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.TwitterOAuthInfoList != null && config.TwitterOAuthInfoList.IsValidIndex(config.TwitterSelectedAccount) &&
            OAuthInfo.CheckOAuth(config.TwitterOAuthInfoList[config.TwitterSelectedAccount]);
    }

    public override URLSharer CreateSharer(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new TwitterSharer(config);
    }
}

public sealed class TwitterSharer : URLSharer
{
    private UploadersConfig config;

    public TwitterSharer(UploadersConfig config)
    {
        this.config = config;
    }

    public override UploadResult ShareURL(string url)
    {
        var result = new UploadResult { URL = url, IsURLExpected = false };

        var twitterOAuth = config.TwitterOAuthInfoList[config.TwitterSelectedAccount];

        if (config.TwitterSkipMessageBox)
        {
            try
            {
                new Twitter(twitterOAuth).TweetMessage(url);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex);
            }
        }
        else
        {
            // TODO: Reimplement TwitterTweetForm
            // using (TwitterTweetForm twitter = new TwitterTweetForm(twitterOAuth, url))
            // {
            //     twitter.ShowDialog();
            // }
        }

        //URLHelpers.OpenURL("https://twitter.com/intent/tweet?text=" + encodedUrl);

        return result;
    }
}

