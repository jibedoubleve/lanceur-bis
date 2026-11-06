namespace Lanceur.Core.Models;

public sealed class AdditionalParameter : ObservableModel
{
    #region Properties

    public long AliasId
    {
        get;
        set => SetField(ref field, value);
    }

    public long Id
    {
        get;
        set => SetField(ref field, value);
    }

    public required string Name
    {
        get;
        set => SetField(ref field, Trim(value));
    }

    public required string Parameter
    {
        get;
        set => SetField(ref field, Trim(value));
    }

    #endregion

    #region Methods

    private static string Trim(string? src) => src?.Trim('"', ' ') ?? string.Empty;

    #endregion
}