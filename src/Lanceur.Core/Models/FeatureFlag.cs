namespace Lanceur.Core.Models;

public class FeatureFlag : ObservableModel
{
    #region Fields

    private string _description;
    private bool _enabled;
    private string _featureName;
    private string _icon;

    #endregion

    #region Properties

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public bool Enabled
    {
        get => _enabled;
        set => SetField(ref _enabled, value);
    }

    public string FeatureName
    {
        get => _featureName;
        set => SetField(ref _featureName, value);
    }

    public string Icon
    {
        get => _icon;
        set => SetField(ref _icon, value);
    }

    #endregion
}