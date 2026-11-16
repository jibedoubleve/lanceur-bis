using System.Data;
using System.Reflection;
using Dapper;

namespace System.SQLite.Updater;

public sealed class DatabaseUpdater
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ScriptManager _scriptManager;

    #endregion

    #region Constructors

    public DatabaseUpdater(IDbConnection db, Assembly asm, string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(asm);

        _db = db ?? throw new ArgumentNullException(nameof(db));

        _scriptManager = new ScriptManager(asm, pattern);
    }

    #endregion

    #region Methods

    /// <remarks>
    ///     Foreign keys are disabled while the scripts run: historical scripts rebuild tables and delete
    ///     rows in an order that would violate referential integrity. The previous setting is restored afterwards.
    /// </remarks>
    private void ExecuteScript(IEnumerable<string> scripts)
    {
        var foreignKeysEnabled = _db.ExecuteScalar<bool>("pragma foreign_keys;");
        _db.Execute("pragma foreign_keys = off;");
        try
        {
            foreach (var script in scripts) { _db.Execute(script); }
        }
        finally
        {
            if (foreignKeysEnabled) { _db.Execute("pragma foreign_keys = on;"); }
        }
    }

    public Version MaxVersion() => _scriptManager.GetScripts().MaxVersion();

    public void UpdateFrom(Version version) => ExecuteScript(_scriptManager.GetScripts().After(version));
    public void UpdateFromScratch() => ExecuteScript(_scriptManager.GetScripts());

    #endregion
}