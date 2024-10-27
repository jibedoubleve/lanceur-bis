namespace Lanceur.Core.Models;

public class UsageQueryResult : QueryResult
{
    #region Properties

    public string Color => Count switch
    {
        > 500 => "green",
        > 100 => "lightgreen",
        > 50  => "orange",
        _     => "red"
    };

    #endregion Properties
}