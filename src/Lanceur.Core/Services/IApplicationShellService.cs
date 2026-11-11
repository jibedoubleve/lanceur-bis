using Lanceur.Core.Models;

namespace Lanceur.Core.Services;

/// <summary>
///     Gives access to the application shell (lifetime and main window) without depending on the UI framework.
/// </summary>
public interface IApplicationShellService
{
    #region Methods

    /// <summary>
    ///     Computes the position that centres the main window on the primary screen.
    /// </summary>
    Coordinate GetMainWindowCentre();

    /// <summary>
    ///     Shuts the application down.
    /// </summary>
    void Shutdown();

    #endregion
}
