using System.Diagnostics;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Services;

namespace Lanceur.Infra.Repositories;

/// <summary>
///     An in-memory implementation of <see cref="ISettingsProvider{T}" /> for <see cref="ApplicationSettings" />.
///     Configuration is never persisted: <see cref="Load" /> and <see cref="Save" /> are no-ops that emit a trace warning.
///     Intended for use in tests and debug scenarios where file I/O is undesirable.
/// </summary>
public sealed class MemoryApplicationSettingsProvider : ISettingsProvider<ApplicationSettings>
{
    #region Properties

    /// <inheritdoc />
    object ISettingsProvider.Current => Current;

    /// <inheritdoc />
    public ApplicationSettings Current { get; } = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Load() =>
        // Does nothing, settings is already in memory
        Trace.TraceWarning("Using memory settings. Nothing will be loaded from file!");

    /// <inheritdoc />
    public void Save() =>
        // Does nothing, settings is already in memory
        Trace.TraceWarning("Using memory settings. Nothing will be saved into file!");

    #endregion
}