namespace Lanceur.Core.Services;

/// <summary>
///     Opens the settings window on a given page, without exposing the views to the caller.
/// </summary>
public interface ISettingsNavigationService
{
    #region Methods

    /// <summary>
    ///     Shows the settings window and navigates to the page bound to <typeparamref name="TViewModel" />.
    /// </summary>
    /// <typeparam name="TViewModel">The view model of the page to display.</typeparam>
    /// <exception cref="NotSupportedException">No page is bound to <typeparamref name="TViewModel" />.</exception>
    void NavigateTo<TViewModel>() where TViewModel : class;

    #endregion
}
