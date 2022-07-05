using Lanceur.Core.Plugins.Models;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Core.Plugins
{
    public abstract class PluginBase : IPlugin
    {
        #region Constructors

        public PluginBase()
        {
            var name = GetType().GetCustomAttributes<PluginAttribute>().FirstOrDefault();
            var description = GetType().GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
            if (name != null) { Name = name.Name; }
            if (description != null) { Description = description.Description; }
            Icon = "FunctionVariant";
        }

        #endregion Constructors

        #region Properties

        protected static IEnumerable<ResultItem> NoResult => new List<ResultItem>();
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }

        #endregion Properties

        #region Methods

        public abstract Task<IEnumerable<ResultItem>> ExecuteAsync(string parameters = null);

        #endregion Methods
    }
}