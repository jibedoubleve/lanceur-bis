using Lanceur.Core.Utils;
using Lanceur.SharedKernel.Mixins;
using System;
using Lanceur.Infra.Constants;

namespace Lanceur.Utils.ConnectionStrings
{
    public class DebugConnectionString : BaseConnectionString, IConnectionString
    {
        #region Fields

        private static string _connectionString;

        private readonly string _dbPath = AppPaths.DefaultDbPath;

        #endregion Fields

        #region Constructors

        private DebugConnectionString(string dbPath)
        {
            _dbPath = dbPath;
        }

        public DebugConnectionString()
        {
        }

        #endregion Constructors

        #region Methods

        public static DebugConnectionString FromFile(string dbPath) => new(dbPath);

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