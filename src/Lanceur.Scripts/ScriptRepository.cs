using System.Reflection;

namespace Lanceur.Scripts;

/// <summary>
///     This class serves the sole purpose of returning the assembly where the SQL scripts are defined.
///     It is used by the Database Update Manager to locate and load SQL scripts.
/// </summary>
public static class ScriptRepository
{
    #region Fields

    /// <summary>
    ///     The pattern used to identify embedded SQL script resources.
    ///     It matches file names in the format: script-X.X.X.sql.
    /// </summary>
    public const string DbScriptEmbeddedResourcePattern = @"Lanceur\.Scripts\.SQL\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the assembly where this class is defined.
    /// </summary>
    public static Assembly Asm => typeof(ScriptRepository).Assembly;

    #endregion
}