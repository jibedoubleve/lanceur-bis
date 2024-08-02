namespace Lanceur.Core.Models;

public class DisplayUsageQueryResult : QueryResult
{
    #region Properties

    public string Color
    {
        get
        {
            return Count switch
            {
                > 500            => "green",
                <= 500 and > 100 => "lightgreen",
                <= 100 and > 50  => "orange",
                _                => "red"
            };
        }
    }

    public override string Icon => Count / 100 > 9
        ? "Numeric9PlusBoxMultipleOutline"
        : $"Numeric{Count / 100}BoxOutline";

    #endregion Properties
}