namespace Lanceur.Core.Models;

public sealed class FeatureFlag : ObservableModel
{
    #region Properties

    public required string Description
    {
        get;
        set => SetField(ref field, value);
    }

    public required bool Enabled
    {
        get => field;
        set => SetField(ref field, value);
    }

    public required string FeatureName
    {
        get;
        set => SetField(ref field, value);
    }

    public required string Icon
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion
}