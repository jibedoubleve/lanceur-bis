namespace Lanceur.Core.Models;

public class AliasUsageItem : ObservableModel
{
    #region Fields

    private string _thumbnail;

    #endregion

    #region Properties

    public string FileName { get; init; }
    public string Icon { get; init; }
    public long Id { get; init; }
    public string Name { get; init; }

    public string Thumbnail
    {
        get => _thumbnail;
        set => SetField(ref _thumbnail, value);
    }

    public DateTime Timestamp { get; init; }

    #endregion
}