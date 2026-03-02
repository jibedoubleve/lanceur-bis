using Lanceur.Core.Models;

namespace Lanceur.Infra.Utils;

public class Doubloon
{
    #region Properties

    public QueryResult Content { get; init; }
    public string Description { get; init; }
    public int HashCode { get; init; }
    public string Name { get; init; }

    #endregion
}

public static class DataDebugger
{
    #region Methods

    public static IEnumerable<Doubloon> GetDoubloons(this IEnumerable<QueryResult> result)
    {
        return result.GroupBy(
                         r => r.GetHashCode(),
                         r => new Doubloon
                         {
                             Name = r.Name,
                             Description = r.Description,
                             Content = r,
                             HashCode = r.GetHashCode()
                         }
                     )
                     .Where(gr => gr.Count() > 1)
                     .Select(gr => gr.Select(t => t).First())
                     .ToArray();
    }

    #endregion
}