namespace Lanceur.Ui.Core.Utils;

public interface IViewFactory
{
    #region Methods

    /// <summary>
    ///     Creates and returns a view displaying version information for the application,
    ///     including version number and commit identifier.
    /// </summary>
    /// <param name="version">The version number of the application (e.g., "1.2.3").</param>
    /// <param name="commit">The commit hash or identifier corresponding to the build.</param>
    /// <returns>An object representing the version information view.</returns>
    object CreateVersionView(string version, string commit);

    /// <summary>
    ///     Creates an instance of the view corresponding to the specified ViewModel
    ///     and sets the DataContext to the provided ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel to associate with the view.</param>
    /// <returns>An object representing the configured view.</returns>
    object CreateView(object viewModel);

    #endregion
}