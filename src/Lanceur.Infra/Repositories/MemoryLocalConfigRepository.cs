using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Services;

namespace Lanceur.Infra.Repositories;

public class MemoryLocalConfigRepository : ILocalConfigRepository
{
    #region Fields

    private static readonly LocalConfig Settings;

    #endregion Fields

    #region Constructors

    static MemoryLocalConfigRepository()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, "debug.sqlite");

        Settings = new() { DbPath = path };
    }

    #endregion Constructors

    #region Properties

    public ILocalConfig Current => Settings;

    #endregion Properties

    #region Methods

    public void Load()
    {
        /*Does nothing, settings is already in memory*/
    }

    public void Save()
    {
        /*Does nothing, settings is already in memory*/
    }

    #endregion Methods
}