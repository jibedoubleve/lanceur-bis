namespace Lanceur.Infra.SQLite.Entities;

public sealed class AdditionalParameterEntity
{
    #region Properties

    public long Id { get; set; }
    public long IdAlias { get; set; }
    public required string Name { get; set; }
    public required string Parameter { get; set; }

    #endregion
}