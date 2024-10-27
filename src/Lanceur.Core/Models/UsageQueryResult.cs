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

    [Obsolete("Should be soon removed.")]
    public override string Icon => Count / 100 > 9
        ? "Numeric9PlusBoxMultipleOutline"
        : $"Numeric{Count / 100}BoxOutline";

    #endregion Properties
}