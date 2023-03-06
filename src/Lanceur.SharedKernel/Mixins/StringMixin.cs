using System.Globalization;
using System.Text.RegularExpressions;

namespace Lanceur.SharedKernel.Mixins
{
    public static class StringMixin
    {
        #region Methods

        public static bool CastToBool(this string @this, bool @default = default) => bool.TryParse(@this, out bool result) ? result : @default;

        public static bool CastToBool(this object @this, bool @default = default) => $"{@this}".CastToBool(@default);

        public static double CastToDouble(this object @this, double @default = default, IFormatProvider provider = null) => $"{@this}".CastToDouble(@default, provider);

        public static double CastToDouble(this string @this, double @default = default, IFormatProvider provider = null)
        {
            provider ??= new CultureInfo("en-US");
            return double.TryParse(@this, NumberStyles.AllowDecimalPoint, provider, out double result) ? result : @default;
        }

        public static int CastToInt(this string @this, int @default = default) => int.TryParse(@this, out int result) ? result : @default;

        public static int CastToInt(this object @this, int @default = default) => $"{@this}".CastToInt(@default);

        public static long CastToLong(this string @this, long @default = default) => long.TryParse(@this, out long result) ? result : @default;

        public static string CastToString(this object @this, string @default = default) => @this is not null ? $"{@this}" : @default;

        public static string ExtractPathFromSQLiteCString(this string value)
        {
            var regex = new Regex("Data Source=(.*);Version=3;");
            return regex.IsMatch(value)
                ? regex.Match(value).Groups[1].Captures[0].Value
                : string.Empty;
        }

        public static string Format(this string format, params object[] args) => string.Format(format, args);

        public static bool IsDirectory(this string value) => Directory.Exists(value);

        public static bool IsFile(this string value) => File.Exists(value);

        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        public static bool IsUwp(this string value) => value?.StartsWith("package:") ?? false;

        public static string ToLowerString(this object value) => value?.ToString()?.ToLower();

        #endregion Methods
    }
}