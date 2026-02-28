namespace Lanceur.SharedKernel.Extensions;

public static class StringExtensions
{
    #region Methods

    public static string DefaultIfNullOrEmpty(this string value, string defaultValue)
        => value.IsNullOrEmpty() ? defaultValue : value;

    public static string Format(this string format, params object[] args) => string.Format(format, args);


    public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

    public static bool IsUwp(this string value) => value?.StartsWith("package:") ?? false;

    public static string JoinCsv(this string[] strings)
    {
        if (strings is null) { return string.Empty; }

        return string.Join(", ", strings);
    }

    public static string[] SplitCsv(this string str)
    {
        if (str is null) { return Array.Empty<string>(); }

        return
            str.Split(",")
               .Select(n => n.Trim())
               .ToArray();
    }

    public static string ToLowerString(this object value) => value?.ToString()?.ToLower();

    #endregion
}