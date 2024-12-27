﻿using Lanceur.Core.Models.Settings;

namespace Lanceur.Core.Repositories.Config;

public interface ISettingsFacade
{
    #region Properties

    AppConfig Application { get; }
    ILocalConfig Local { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Save all the changes into the disk
    /// </summary>
    void Save();

    /// <summary>
    /// Reload the settings from the disk
    /// </summary>
    void Reload();

    #endregion Methods
}