namespace Lanceur.Core.Models
{

    public class PluginQueryResult : QueryResult
    {
        #region Fields

        private string _result;

        #endregion Fields

        #region Properties

        public override string Description => _result;

        #endregion Properties

        #region Methods

        public void SetDescription(string value)
        {
            _result = value;
        }

        #endregion Methods
    }
}