namespace Lanceur.Core.Models
{
    /// <summary>
    /// Get a query that can be casted to a <see cref="Cmdline"/>
    /// </summary>
    public interface IQueryText
    {
        string ToQuery();
    }
}