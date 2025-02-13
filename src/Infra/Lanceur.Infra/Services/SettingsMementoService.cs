using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;

namespace Lanceur.Infra.Services;

public class SettingsMementoService
{
    #region Fields

    private readonly int _stateHash;

    #endregion Fields

    #region Constructors

    private SettingsMementoService(int initialHash) => _stateHash = initialHash;

    #endregion Constructors

    #region Methods

    private static int GetStateHash(DatabaseConfiguration appCfg, IApplicationSettings dbCfg) => (appCfg.HotKey, dbCfg.DbPath).GetHashCode();

    public static SettingsMementoService GetInitialState(ISettingsFacade settings) => new(GetStateHash(settings.Application, settings.Local));

    public bool HasStateChanged(ISettingsFacade settings) => GetStateHash(settings.Application, settings.Local) != _stateHash;

    #endregion Methods
}