namespace Lanceur.Infra.SQLite.Entities;

public class AdditionalParameterEntity
{
    #region Properties

    public long Id { get; set; }
    public long IdAlias { get; set; }
    public required string Name { get; set; }
    public required string Parameter { get; set; }

    #endregion
}