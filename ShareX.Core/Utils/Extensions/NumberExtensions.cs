
// SPDX-License-Identifier: GPL-3.0-or-later



namespace ShareX.Core.Utils.Extensions;

public static class NumberExtensions
{
    private static readonly string[] suffixDecimal = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
    private static readonly string[] suffixBinary = new[] { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };

    public static T Min<T>(this T num, T min) where T : IComparable<T>
    {
        return Math.Min(num, min);
    }

    public static T Max<T>(this T num, T max) where T : IComparable<T>
    {
        return Math.Max(num, max);
    }

    public static T Clamp<T>(this T num, T min, T max) where T : IComparable<T>
    {
        return Math.Clamp(num, min, max);
    }

    public static bool IsBetween<T>(this T num, T min, T max) where T : IComparable<T>
    {
        return Math.IsBetween(num, min, max);
    }

    public static T BetweenOrDefault<T>(this T num, T min, T max, T defaultValue = default) where T : IComparable<T>
    {
        return Math.BetweenOrDefault(num, min, max, defaultValue);
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return Math.Remap(value, from1, to1, from2, to2);
    }

    public static bool IsEvenNumber(this int num)
    {
        return Math.IsEvenNumber(num);
    }

    public static bool IsOddNumber(this int num)
    {
        return Math.IsOddNumber(num);
    }

    public static string ToSizeString(this long size, bool binary = false, int decimalPlaces = 2)
    {
        int bytes = binary ? 1024 : 1000;
        if (size < bytes) return System.Math.Max(size, 0) + " B";
        int place = (int)System.Math.Floor(System.Math.Log(size, bytes));
        double num = size / System.Math.Pow(bytes, place);
        string suffix = binary ? suffixBinary[place] : suffixDecimal[place];
        return num.ToDecimalString(decimalPlaces.Clamp(0, 3)) + " " + suffix;
    }

    public static string ToDecimalString(this double number, int decimalPlaces)
    {
        string format = "0";
        if (decimalPlaces > 0) format += "." + new string('0', decimalPlaces);
        return number.ToString(format);
    }

    public static string ToBase(this int value, int radix, string digits)
    {
        if (string.IsNullOrEmpty(digits))
        {
            throw new ArgumentNullException("digits", string.Format("Digits must contain character value representations"));
        }

        radix = System.Math.Abs(radix);
        if (radix > digits.Length || radix < 2)
        {
            throw new ArgumentOutOfRangeException("radix", radix, string.Format("Radix has to be > 2 and < {0}", digits.Length));
        }

        string result = "";
        int quotient = System.Math.Abs(value);
        while (quotient > 0)
        {
            int temp = quotient % radix;
            result = digits[temp] + result;
            quotient /= radix;
        }
        return result;
    }
}
