namespace Lanceur.Ui.WPF.Helpers;

public class LazyLoadedSynchronizationContext
{
    #region Methods

    /// <summary>
    ///     Lazily retrieves the application's synchronization context.
    ///     Ensures the WPF application is fully loaded before accessing it.
    /// </summary>
    /// <returns>The application's synchronization context.</returns>
    /// <exception cref="NullReferenceException">Thrown if the synchronization context is not yet available.</exception>
    public SynchronizationContext Current 
        => App.UiContext 
           ?? throw new NullReferenceException("SynchronizationContext.Current is null.");

    #endregion
}