using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Splat;

namespace Lanceur.Core.Plugins
{
    public abstract class PluginBase : ExecutableQueryResult, IPlugin
    {
        #region Fields

        private readonly ILogService _log;

        #endregion Fields

        #region Constructors

        public PluginBase()
        {
            _log = Locator.Current.GetService<ILogService>() ?? new TraceLogService();
        }

        #endregion Constructors

        #region Properties

        protected ILogService Log => _log;

        #endregion Properties
    }
}