using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.Utils;

public abstract class QueryResultHelper
{
    #region Methods

    public static IEnumerable<AliasQueryResult> CreateEnumeration(int length)
    {
        var result = new List<AliasQueryResult>();
        for (var i = 0; i < length; i++) result.Add(new());
        return result.ToArray();
    }

    #endregion
}