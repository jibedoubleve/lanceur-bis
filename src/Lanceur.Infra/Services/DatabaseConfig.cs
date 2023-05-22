using Lanceur.Core.Models.Settings;

namespace Lanceur.Infra.Services
{
    public class DatabaseConfig : IDatabaseConfig
    {
        #region Properties

        public string DbPath { get; set; } = @"%appdata%\probel\lanceur2\data.sqlite";

        #endregion Properties
    }
}