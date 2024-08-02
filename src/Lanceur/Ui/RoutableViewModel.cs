using ReactiveUI;

namespace Lanceur.Ui;

public abstract class RoutableViewModel : ReactiveObject, IRoutableViewModel
{
    #region Properties

    public IScreen HostScreen { get; protected set; }
    public string UrlPathSegment => GetType().Name;

    #endregion Properties
}