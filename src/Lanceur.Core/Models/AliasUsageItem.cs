namespace Lanceur.Core.Models;

public sealed class AliasUsageItem : ObservableModel
{
    #region Properties

    public required string FileName { get; init; }
    public required string Icon { get; init; }
    public required long Id { get; init; }
    public required string Name { get; init; }

    public string? Thumbnail
    {
        get;
        set => SetField(ref field, value);
    }

    public required DateTime Timestamp { get; init; }

    #endregion
}