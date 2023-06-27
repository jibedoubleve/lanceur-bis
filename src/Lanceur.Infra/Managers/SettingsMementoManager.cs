using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Managers
{
    public class SettingsMementoManager
    {
        #region Fields

        public int _stateHash;

        #endregion Fields

        #region Constructors

        private SettingsMementoManager(int initialHash)
        {
            _stateHash = initialHash;
        }

        #endregion Constructors

        #region Methods

        private static int GetStateHash(AppConfig appCfg, IDatabaseConfig dbCfg) => (appCfg.HotKey, dbCfg.DbPath).GetHashCode();

        public static SettingsMementoManager InitialState(ISettingsFacade settings) => new(GetStateHash(settings.Application, settings.Database));

        public bool HasStateChanged(ISettingsFacade settings) => GetStateHash(settings.Application, settings.Database) != _stateHash;

        #endregion Methods
    }
}