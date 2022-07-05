namespace Lanceur.Core.Models
{
    public class DataPoint<Tx, Ty>
    {
        #region Properties

        public Tx X { get; set; }
        public Ty Y { get; set; }

        #endregion Properties
    }
}