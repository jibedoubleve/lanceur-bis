using System.Reflection;

namespace Lanceur.Scripts;

/// <summary>
/// The sole purpose of this class is to return the
/// Assembly where it stands. Thi sis used in the
/// <see cref="SQLiteDatabaseUpdateManager"/> to find
/// and load the SQL scripts
/// </summary>
public static class ScriptRepository
{
    #region Fields

    public const string DbScriptEmbededResourcePattern = @"Lanceur\.Scripts\.SQL\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

    #endregion Fields

    #region Properties

    public static Assembly Asm => typeof(ScriptRepository).Assembly;

    #endregion Properties
}