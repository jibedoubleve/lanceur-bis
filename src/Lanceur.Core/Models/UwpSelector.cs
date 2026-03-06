namespace Lanceur.Core.Models;

public sealed class UwpSelector : ObservableModel
{
    #region Properties

    public IEnumerable<PackagedApp>? PackagedApps { get; set; }

    public PackagedApp? SelectedPackagedApp
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion
}