namespace Lanceur.Ui.Core.Utils;

public interface IViewFactory
{
    #region Methods

    /// <summary>
    /// Creates an instance of the view corresponding to the specified ViewModel 
    /// and sets the DataContext to the provided ViewModel.
    /// </summary>
    /// <param name="viewModel">The ViewModel to associate with the view.</param>
    /// <returns>An object representing the configured view.</returns>
    object CreateView(object viewModel);
    
    #endregion
}