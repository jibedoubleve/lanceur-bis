namespace Lanceur.Core.Models;

public sealed class QueryResultAdditionalParameters
{
    public long Id { get; set; }
    
    public long AliasId { get; set; }
    public string Name { get; set; }
    public string Parameter { get; set; }
}