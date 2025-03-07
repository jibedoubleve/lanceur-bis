using System.Globalization;

namespace Lanceur.SharedKernel.Extensions;

/// <summary>
/// All related extension about type management un SQLite
/// </summary>
public static class SQLiteTypeExtensions
{
    /// <summary>
    /// Parse a datetime into a string understanda
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string ToSQLiteDate(this DateTime date) => date.ToString("o", CultureInfo.InvariantCulture);
}