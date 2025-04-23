namespace Lanceur.Core.Models;

public class UwpSelector : ObservableModel
{
    #region Fields

    private PackagedApp _selectedPackagedApp;

    #endregion

    #region Properties

    public IEnumerable<PackagedApp> PackagedApps { get; set; }

    public PackagedApp SelectedPackagedApp
    {
        get => _selectedPackagedApp;
        set => SetField(ref _selectedPackagedApp, value);
    }

    #endregion
}