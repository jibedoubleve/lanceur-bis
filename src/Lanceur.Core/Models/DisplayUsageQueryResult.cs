namespace Lanceur.Core.Models
{
    public class DisplayUsageQueryResult : QueryResult
    {
        public override string Icon
        {
            get
            {
                return (Count / 100) > 9 
                    ? "Numeric9PlusBoxMultipleOutline" 
                    : $"Numeric{Count / 100}BoxOutline";
            }
        }
        public string Color
        {
            get
            {
                return Count switch
                {
                    > 500 => "green",
                    <= 500 and > 100 => "lightgreen",
                    <= 100 and > 50 => "orange",
                    _ => "red",
                };
            }
        }
    }
}