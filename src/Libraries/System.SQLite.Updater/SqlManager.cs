using System.Data;
using Dapper;

namespace System.SQLite.Updater
{
    public class SqlManager
    {
        #region Fields

        private readonly IDbConnection _db;

        #endregion Fields

        #region Constructors

        public SqlManager(IDbConnection db)
        {
            _db = db;
        }

        #endregion Constructors

        #region Methods

        public void Execute(IEnumerable<string> sqlScripts)
        {
            foreach (var script in sqlScripts)
            {
                _db.Execute(script);
            }
        }

        #endregion Methods
    }
}