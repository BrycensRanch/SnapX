
// SPDX-License-Identifier: GPL-3.0-or-later


using SnapX.Core.Utils;

namespace SnapX.Core;

public static class SnapXResources
{
    public static string Name => "SnapX";

    public static string UserAgent
    {
        get
        {
            return $"{Name}/{Helpers.GetApplicationVersion()}";
        }
    }
}

