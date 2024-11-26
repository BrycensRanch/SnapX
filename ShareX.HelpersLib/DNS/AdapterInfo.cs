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
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/
using System.Net.NetworkInformation;

#endregion License Information (GPL v3)

namespace ShareX.HelpersLib
{
    public class AdapterInfo : IDisposable
    {
        private NetworkInterface adapter;

        public AdapterInfo(NetworkInterface adapter)
        {
            this.adapter = adapter;
        }

        public static List<AdapterInfo> GetEnabledAdapters()
        {
            var adapters = new List<AdapterInfo>();
            var enabledInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .ToList();

            foreach (var ni in enabledInterfaces)
            {
                adapters.Add(new AdapterInfo(ni));
            }
            return adapters;
        }

        public bool IsEnabled() => adapter.OperationalStatus == OperationalStatus.Up;

        public string? GetCaption() => adapter.ToString();

        public string GetDescription() => adapter.Description;

        public string[] GetDNS() => adapter.GetIPProperties().UnicastAddresses.ToList().Select(x => x.Address.ToString()).ToArray();
        // TODO: SetDNS needs to be implemented on a per platform basis
        public uint SetDNS(string primary, string secondary) => throw new NotImplementedException("SetDNS is not implemented.");
        public uint SetDNSAutomatic()
        {
            return SetDNS(null, null);
        }

        public void Dispose() => Console.WriteLine($"Disposed adapter {adapter.Description}");

        public override string ToString() => GetDescription();
    }
}
