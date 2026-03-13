using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;

namespace Lanceur.Core.Configuration;

/// <summary>
///     Facade that provides unified access to all application settings providers.
///     Aggregates <see cref="ApplicationSettings" /> and <see cref="InfrastructureSettings" />
///     behind a single interface, hiding the individual <see cref="ISettingsProvider{TConfig}" /> instances.
/// </summary>
public interface ISettingsProviderFacade
{
    #region Properties

    /// <summary>
    ///     Gets the current application-level configuration (UI behaviour, hotkeys, feature flags, etc.).
    /// </summary>
    ApplicationSettings Application { get; }

    /// <summary>
    ///     Gets the current infrastructure-level configuration (database path, telemetry, etc.).
    /// </summary>
    InfrastructureSettings Infrastructure { get; }

    #endregion

    #region Methods

    /// <summary>
    ///     Persists all settings by delegating to each underlying <see cref="ISettingsProvider{TConfig}.Save" />.
    /// </summary>
    void Save();

    #endregion
}

/// <inheritdoc />
public class SettingsProviderFacade : ISettingsProviderFacade
{
    #region Fields

    private readonly ISettingsProvider<InfrastructureSettings> _infrastructureSettingsProvider;
    private readonly ISettingsProvider<ApplicationSettings> _settingsProvider;

    #endregion

    #region Constructors

    public SettingsProviderFacade(
        ISettingsProvider<ApplicationSettings> settingsProvider,
        ISettingsProvider<InfrastructureSettings> infrastructureSettingsProvider)
    {
        _settingsProvider = settingsProvider;
        _infrastructureSettingsProvider = infrastructureSettingsProvider;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public ApplicationSettings Application => _settingsProvider.Current;

    /// <inheritdoc />
    public InfrastructureSettings Infrastructure => _infrastructureSettingsProvider.Current;

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Save()
    {
        _settingsProvider.Save();
        _infrastructureSettingsProvider.Save();
    }

    #endregion
}