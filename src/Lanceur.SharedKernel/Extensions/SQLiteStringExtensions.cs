using System.Text.RegularExpressions;

namespace Lanceur.SharedKernel.Extensions;

public static class SQLiteStringExtensions
{
    #region Methods

    /// <summary>
    /// Extracts the file path of the SQLite database from a connection string.
    /// </summary>
    /// <param name="value">The SQLite connection string from which to extract the database path.</param>
    /// <returns>The database file path if found; otherwise, an empty string.</returns>
    public static string ExtractPathFromSQLiteCString(this string value)
    {
        var regex = new Regex("Data Source=(.*);Version=3;");
        return regex.IsMatch(value)
            ? regex.Match(value).Groups[1].Captures[0].Value
            : string.Empty;
    }

    /// <summary>
    /// Generates a SQLite connection string based on the provided database file path.
    /// </summary>
    /// <param name="value">The file path of the SQLite database.</param>
    /// <returns>A valid SQLite connection string formatted with the specified file path.</returns>
    /// <remarks>
    /// This method performs no validation of the input value; it simply formats the string as a SQLite connection string.
    /// </remarks>
    public static string ToSQLiteConnectionString(this string value) => $"Data Source={value};Version=3;";

    #endregion
}