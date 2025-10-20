using Lanceur.Core.Configuration.Configurations;

namespace Lanceur.Core.Repositories.Config;

public interface ISettingsFacade
{
    #region Properties

    DatabaseConfiguration Application { get; }
    ApplicationConfiguration Local { get; }

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

    /// <summary>
    ///     Occurs when the configuration is reloaded or modified
    /// </summary>
    event EventHandler Updated;
}