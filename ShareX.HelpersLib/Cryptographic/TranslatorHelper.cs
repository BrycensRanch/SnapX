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

using System.Text;
using System.Text.RegularExpressions;

namespace ShareX.HelpersLib
{
    public static class TranslatorHelper
    {

        public static string[] TextToBinary(string text) =>
            text.Select(c => ByteToBinary((byte)c)).ToArray();
        public static string[] TextToHexadecimal(string text) => BytesToHexadecimal(Encoding.UTF8.GetBytes(text));

        public static byte[] TextToASCII(string text) => Encoding.ASCII.GetBytes(text);

        public static string TextToBase64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

        public static string TextToHash(string text, HashType hashType, bool uppercase = false)
        {
            using var hash = HashChecker.GetHashAlgorithm(hashType);
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(text));
            var hex = BytesToHexadecimal(bytes);
            var result = string.Concat(hex);
            if (uppercase) result = result.ToUpperInvariant();
            return result;
        }


        public static byte BinaryToByte(string binary) => Convert.ToByte(binary, 2);
        public static string BinaryToText(string binary)
        {
            binary = Regex.Replace(binary, @"[^01]", "");
            using var stream = new MemoryStream();
            foreach (var i in Enumerable.Range(0, binary.Length / 8))
            {
                stream.WriteByte(BinaryToByte(binary.Substring(i * 8, 8)));
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static string ByteToBinary(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

        public static string[] BytesToHexadecimal(byte[] bytes) =>
            bytes.Select(b => b.ToString("x2")).ToArray();

        public static byte HexadecimalToByte(string hex) => Convert.ToByte(hex, 16);

        public static string HexadecimalToText(string hex)
        {
            hex = Regex.Replace(hex, @"[^0-9a-fA-F]", "");
            var byteCount = hex.Length / 2;
            var buffer = new byte[byteCount];

            foreach (var i in Enumerable.Range(0, byteCount))
            {
                buffer[i] = HexadecimalToByte(hex.Substring(i * 2, 2));
            }

            return Encoding.UTF8.GetString(buffer);
        }

        public static string Base64ToText(string base64) => Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        public static string ASCIIToText(string ascii)
        {
            var bytes = ascii
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => byte.TryParse(s, out _))
                .Select(s => byte.Parse(s))
                .ToArray();

            return Encoding.ASCII.GetString(bytes);
        }

    }
}
