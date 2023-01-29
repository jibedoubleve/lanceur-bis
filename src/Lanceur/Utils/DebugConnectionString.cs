using Lanceur.Core.Utils;
using Lanceur.Infra.Stores;
using Lanceur.SharedKernel.Mixins;
using System;

namespace Lanceur.Utils
{
    public class DebugConnectionString : BaseConnectionString, IConnectionString
    {
        #region Fields

        private readonly string _dbPath = @"%appdata%\probel\lanceur2\data.sqlite";
        //private string _dbPath = @"%appdata%\probel\Lanceur\debug_data.db";
        private static string _connectionString;

        #endregion Fields
        public DebugConnectionString()
        {

        }
        #region Methods

        public override string ToString()
        {
            if (_connectionString is null)
            {
                var path = Environment.ExpandEnvironmentVariables(_dbPath);
                _connectionString = CSTRING_PATTERN.Format(path);
            }
            return _connectionString;
        }

        #endregion Methods
    }
}