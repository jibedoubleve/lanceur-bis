using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace System.SQLite.Updater
{
    public class DatabaseUpdater
    {
        #region Fields

        private readonly Assembly _asm;
        private readonly IDbConnection _db;
        private readonly string _pattern;
        private readonly ScriptManager _scriptManager;
        private readonly SqlManager _sqlManager;

        #endregion Fields

        #region Constructors

        public DatabaseUpdater(IDbConnection db, Assembly asm, string pattern)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _asm = asm ?? throw new ArgumentNullException(nameof(asm));
            _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));

            _sqlManager = new SqlManager(_db);
            _scriptManager = new ScriptManager(_asm, _pattern);
        }

        #endregion Constructors

        #region Methods

        public Version MaxVersion() => _scriptManager.GetScripts().MaxVersion();

        public void UpdateFrom(Version version)
        {
            var scripts = _scriptManager.GetScripts();
            _sqlManager.Execute(scripts.After(version));
        }

        public void UpdateFromScratch()
        {
            var scripts = _scriptManager.GetScripts();
            _sqlManager.Execute(scripts);
        }

        #endregion Methods
    }
}