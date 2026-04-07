namespace Lanceur.Core.Models;

public sealed record AliasUsage
{
    #region Properties

    public int Count { get; set; }

    /// <summary>
    ///     Fallback icon displayed when no thumbnail is available for this alias.
    ///     A <c>null</c> value indicates that no icon is available, not that an error occurred.
    /// </summary>
    public required string? Icon { get; set; }

    public required string Name { get; set; }

    /// <summary>
    ///     Path to the cached thumbnail image for this alias.
    ///     A <c>null</c> value indicates that no thumbnail is available, not that an error occurred.
    /// </summary>
    public required string? Thumbnail { get; set; }

    public required int Year { get; set; }

    #endregion
}