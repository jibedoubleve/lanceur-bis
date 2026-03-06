namespace Lanceur.Core.Models;

public sealed record PackagedApp
{
    #region Properties

    public required string? AppUserModelId { get; init; }
    public required string Description { get; init; }
    public required string DisplayName { get; init; }
    public string FileName => $"package:{AppUserModelId}";
    public required string InstalledLocation { get; init; }
    public required Uri Logo { get; init; }

    #endregion
}