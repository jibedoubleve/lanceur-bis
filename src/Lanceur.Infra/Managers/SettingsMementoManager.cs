using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Managers
{
    public class SettingsMementoManager
    {
        #region Fields

        private readonly int _stateHash;

        #endregion Fields

        #region Constructors

        private SettingsMementoManager(int initialHash)
        {
            _stateHash = initialHash;
        }

        #endregion Constructors

        #region Methods

        private static int GetStateHash(AppConfig appCfg, ILocalConfig dbCfg) => (appCfg.HotKey, dbCfg.DbPath).GetHashCode();

        public static SettingsMementoManager GetInitialState(ISettingsFacade settings) => new(GetStateHash(settings.Application, settings.Local));

        public bool HasStateChanged(ISettingsFacade settings) => GetStateHash(settings.Application, settings.Local) != _stateHash;

        #endregion Methods
    }
}