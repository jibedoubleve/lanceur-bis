using Lanceur.Core.Plugins.Models;

namespace Lanceur.Core.Plugins
{
    public interface IPlugin
    {
        #region Properties

        string Description { get; }
        string Icon { get; set; }
        string Name { get; }

        #endregion Properties

        #region Methods

        Task<IEnumerable<ResultItem>> ExecuteAsync(string parameters = null);

        #endregion Methods
    }
}