#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System.Net;
using System.Net.NetworkInformation;

namespace ShareX.Core.Utils.Miscellaneous;
public static class PingHelper
{
    public static PingResult PingHost(string host, int timeout = 1000, int pingCount = 4, int waitTime = 100)
    {
        var pingResult = new PingResult();
        var address = GetIpFromHost(host);
        var buffer = new byte[32];
        var pingOptions = new PingOptions(128, true);

        using var ping = new Ping();
        for (int i = 0; i < pingCount; i++)
        {
            try
            {
                var pingReply = ping.Send(address, timeout, buffer, pingOptions);
                if (pingReply == null) continue;

                pingResult.PingReplyList.Add(pingReply);
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            if (waitTime > 0 && i + 1 < pingCount)
            {
                Thread.Sleep(waitTime);
            }
        }

        return pingResult;
    }

    private static IPAddress GetIpFromHost(string host)
    {
        if (!IPAddress.TryParse(host, out IPAddress address))
        {
            try
            {
                address = Dns.GetHostEntry(host).AddressList[0];
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }
        }

        return address;
    }
}

