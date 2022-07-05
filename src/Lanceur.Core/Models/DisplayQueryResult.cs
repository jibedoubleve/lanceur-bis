namespace Lanceur.Core.Models
{
    public class DisplayQueryResult : QueryResult
    {
        #region Fields

        private readonly string _description;

        #endregion Fields

        #region Constructors

        public DisplayQueryResult(string name, string description = null, string iconKind = null)
        {
            Name = name;
            _description = description;
            Icon = iconKind;
        }

        #endregion Constructors

        #region Properties

        public override string Description => _description;

        public override bool IsResult => false;

        #endregion Properties

        #region Methods

        public static IEnumerable<QueryResult> SingleFromResult(string text, string subtext = null, string iconKind = null)
        {
            return new List<QueryResult>
            {
                new DisplayQueryResult(text, subtext, iconKind)
            };
        }

        #endregion Methods
    }
}