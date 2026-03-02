namespace Lanceur.Infra.SQLite.Entities;

public class AdditionalParameterEntity
{
    #region Properties

    public long Id { get; set; }
    public long IdAlias { get; set; }
    public string Name { get; set; }
    public string Parameter { get; set; }

    #endregion
}