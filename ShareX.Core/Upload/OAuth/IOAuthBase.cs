
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.Core.Upload.OAuth;

public interface IOAuthBase
{
    string GetAuthorizationURL();

    bool GetAccessToken(string code);
}

