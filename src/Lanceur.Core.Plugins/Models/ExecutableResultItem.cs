namespace Lanceur.Core.Plugins.Models
{
    public class ExecutableResultItem : ResultItem
    {
        #region Fields

        private readonly IPlugin _plugin;

        #endregion Fields

        #region Constructors 

        public ExecutableResultItem(IPlugin plugin) : base(plugin)
        {
            _plugin = plugin;
        }

        #endregion Constructors

        #region Methods

        public async Task<IEnumerable<ResultItem>> ExecuteAsync(string parameters = null) => await _plugin?.ExecuteAsync(parameters);

        #endregion Methods
    }
}