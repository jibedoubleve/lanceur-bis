using Lanceur.Core.Models.Settings;
using Lanceur.Infra.Constants;

namespace Lanceur.Infra.Services
{
    public class DatabaseConfig : IDatabaseConfig
    {
        #region Properties

        public string DbPath { get; set; } = AppPaths.DefaultDbPath;

        #endregion Properties
    }
}