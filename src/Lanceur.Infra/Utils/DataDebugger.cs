using Lanceur.Core.Models;

namespace Lanceur.Infra.Utils;

public class Doubloon
{
    #region Properties

    public QueryResult Content { get; init; }
    public string Description { get; init; }
    public int HashCode { get; init; }
    public string Name { get; init; }

    #endregion Properties
}

public static class DataDebugger
{
    #region Methods

    public static IEnumerable<Doubloon> GetDoubloons(this IEnumerable<QueryResult> result)
    {
        return (from r in result
                group new Doubloon
                {
                    Name = r.Name,
                    Description = r.Description,
                    Content = r,
                    HashCode = r.GetHashCode()
                } by r.GetHashCode()
                into gr
                where gr.Count() > 1
                select gr.Select(t => t)
                         .First()).ToArray();
    }

    #endregion Methods
}