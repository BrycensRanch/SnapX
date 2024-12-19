
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils;

namespace ShareX.Core;

public static class ShareXResources
{
    public static string Name => "ShareX-Linux";

    public static string UserAgent
    {
        get
        {
            return $"{Name}/{Helpers.GetApplicationVersion()}";
        }
    }
}

