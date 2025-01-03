namespace Lanceur.Core.Models;

public sealed class AdditionalParameter : ObservableModel
{
    #region Fields

    private long _aliasId;
    private long _id;
    private string _name;
    private string _parameter;

    #endregion

    #region Properties

    public long AliasId
    {
        get => _aliasId;
        set => SetField(ref _aliasId, value);
    }

    public long Id
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string Parameter
    {
        get => _parameter;
        set => SetField(ref _parameter, value);
    }

    #endregion
}