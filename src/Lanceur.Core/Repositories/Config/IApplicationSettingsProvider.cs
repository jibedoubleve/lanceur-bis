using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;

namespace Lanceur.Core.Repositories.Config;

/// <summary>
///     Defines a contract for managing application infrastructure configuration settings.
/// </summary>
public interface IApplicationSettingsProvider : ISettingsProvider<ApplicationSettings>
{
    #region Methods

    /// <summary>
    ///     Applies modifications to the application settings and saves them in a single operation.
    /// </summary>
    /// <param name="edit">
    ///     A delegate that defines the modifications to be applied to the <see cref="ApplicationSettings" />
    ///     instance.
    /// </param>
    /// <remarks>
    ///     This applies the modifications specified by the <paramref name="edit" /> delegate, and persists the changes.
    /// </remarks>
    void Edit(Action<ApplicationSettings> edit);

    #endregion
}