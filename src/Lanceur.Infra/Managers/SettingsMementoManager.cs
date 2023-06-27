using Lanceur.Core.Models.Settings;

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

        private static int GetStateHash(AppConfig state, string dbPath) => (state.HotKey, dbPath).GetHashCode();

        public static SettingsMementoManager InitialState(AppConfig initialState, string dbPath) => new(GetStateHash(initialState, dbPath));

        public bool HasStateChanged(AppConfig newState, string dbPath) => GetStateHash(newState, dbPath) != _stateHash;

        #endregion Methods
    }
}