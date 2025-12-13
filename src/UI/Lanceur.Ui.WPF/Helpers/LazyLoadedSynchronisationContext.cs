namespace Lanceur.Ui.WPF.Helpers;

public class LazyLoadedSynchronisationContext
{
    #region Methods

    /// <summary>
    ///     Lazily retrieves the application's synchronisation context.
    ///     Ensures the WPF application is fully loaded before accessing it.
    /// </summary>
    /// <returns>The application's synchronisation context.</returns>
    /// <exception cref="NullReferenceException">Thrown if the synchronisation context is not yet available.</exception>
    public SynchronizationContext Current 
        => App.UiContext 
           ?? throw new InvalidOperationException("SynchronizationContext.Current is null.");

    #endregion
}