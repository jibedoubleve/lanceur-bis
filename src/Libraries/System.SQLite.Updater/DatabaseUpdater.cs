using System.Data;
using System.Reflection;
using Dapper;

namespace System.SQLite.Updater;

public class DatabaseUpdater
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

        _scriptManager = new(asm, pattern);
    }

    #endregion

    #region Methods

    private void ExecuteScript(IEnumerable<string> scripts)
    {
        foreach (var script in scripts) _db.Execute(script);
    }

    public Version MaxVersion() => _scriptManager.GetScripts().MaxVersion();

    public void UpdateFrom(Version version) => ExecuteScript(_scriptManager.GetScripts().After(version));
    public void UpdateFromScratch() => ExecuteScript(_scriptManager.GetScripts());

    #endregion
}