using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;

namespace Lanceur.Core.Repositories.Config;

/// <summary>
///     Provides functionality to modify the database configuration settings.
///     These settings are stored directly within the database and define key behaviours and preferences.
/// </summary>
public interface IDatabaseConfigurationService : IConfigurationService<DatabaseConfiguration>
{
    #region Methods

    /// <summary>
    ///     Applies modifications to the database configuration.
    /// </summary>
    /// <param name="action">A delegate that defines the modifications to be applied.</param>
    void Edit(Action<DatabaseConfiguration> action);

    #endregion
}