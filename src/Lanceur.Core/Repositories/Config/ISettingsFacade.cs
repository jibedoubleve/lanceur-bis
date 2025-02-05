using Lanceur.Core.Models.Settings;

namespace Lanceur.Core.Repositories.Config;

public interface ISettingsFacade
{
    #region Properties

    DatabaseConfiguration Application { get; }
    IApplicationSettings Local { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reload the settings from the disk
    /// </summary>
    void Reload();

    /// <summary>
    ///     Save all the changes into the disk
    /// </summary>
    void Save();

    #endregion
}