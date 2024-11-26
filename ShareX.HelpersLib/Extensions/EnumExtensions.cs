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

using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace ShareX.HelpersLib
{
    public static class EnumExtensions
    {

        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            return field?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? value.ToString();
        }

        public static string GetLocalizedDescription(this Enum value) => value.GetDescription();
        // TODO: Implement Localization
        public static string GetLocalizedDescription(this Enum value, ResourceManager resourceManager) =>
            new NotImplementedException("GetLocalizedCategory is not implemented due to no localization existing yet.").Message;


        public static string GetLocalizedCategory(this Enum value) =>
            new NotImplementedException("GetLocalizedCategory is not implemented due to no localization existing yet.").Message;

        public static int GetIndex(this Enum value) => Array.IndexOf(Enum.GetValues(value.GetType()) as Array, value);

        public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, Enum =>
            Enum.GetValues<T>()
            .Where(flag => Convert.ToUInt64(flag) != 0 && value.HasFlag(flag));


        public static bool HasFlag<T>(this Enum value, params T[] flags)
        {
            var keysVal = Convert.ToUInt64(value);
            var flagVal = flags.Select(x => Convert.ToUInt64(x)).Aggregate((x, next) => x | next);
            return (keysVal & flagVal) == flagVal;
        }

        public static bool HasFlagAny<T>(this Enum value, params T[] flags) => flags.Any(x => value.HasFlag(x));
        public static T Add<T>(this Enum value, params T[] flags) where T : Enum
        {
            var result = Convert.ToUInt64(value);
            result |= flags.Select(flag => Convert.ToUInt64(flag)).Aggregate(result, (current, next) => current | next);
            return (T)Enum.ToObject(typeof(T), result);
        }


        public static T Remove<T>(this Enum value, params T[] flags)
        {
            var keysVal = Convert.ToUInt64(value);
            var flagVal = flags.Select(x => Convert.ToUInt64(x)).Aggregate((x, next) => x | next);
            return (T)Enum.ToObject(typeof(T), keysVal & ~flagVal);
        }

        public static T Swap<T>(this Enum value, params T[] flags)
        {
            var keysVal = Convert.ToUInt64(value);
            var flagVal = flags.Select(x => Convert.ToUInt64(x)).Aggregate((x, next) => x | next);
            return (T)Enum.ToObject(typeof(T), keysVal ^ flagVal);
        }

        public static T Next<T>(this Enum value)
        {
            var values = Enum.GetValues(value.GetType());
            var i = Array.IndexOf(values, value) + 1;
            return i == values.Length ? (T)values.GetValue(0) : (T)values.GetValue(i);
        }

        public static T Previous<T>(this Enum value)
        {
            var values = Enum.GetValues(value.GetType());
            var i = Array.IndexOf(values, value) - 1;
            return i == -1 ? (T)values.GetValue(values.Length - 1) : (T)values.GetValue(i);
        }
    }
}
