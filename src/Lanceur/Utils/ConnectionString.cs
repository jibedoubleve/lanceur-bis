using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.Infra.Stores;
using Lanceur.SharedKernel.Mixins;
using System;
using System.IO;

namespace Lanceur.Utils
{
    public class ConnectionString : BaseConnectionString, IConnectionString
    {
        #region Fields

        private readonly string _dbPath;

        #endregion Fields

        #region Constructors

        public ConnectionString(ISettingsService stg)
        {
            _dbPath = Environment.ExpandEnvironmentVariables(stg[Setting.DbPath]);
        }

        public ConnectionString(string path = null)
        {
            _dbPath = Environment.ExpandEnvironmentVariables(path) ?? throw new InvalidDataException($"Cannot find settings for DBPATH or cannot cast in into a string.");
        }

        #endregion Constructors

        #region Methods

        public override string ToString()
        {
            if (!File.Exists(_dbPath))
            {
                LogService.Current.Warning($"The path '{_dbPath}' doesn't exist. A new database should be created!");
            }
            var path = Environment.ExpandEnvironmentVariables(_dbPath);
            return CSTRING_PATTERN.Format(path);
        }

        #endregion Methods
    }
}