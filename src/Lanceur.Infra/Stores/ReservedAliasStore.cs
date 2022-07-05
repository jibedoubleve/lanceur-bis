using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class ReservedAliasStore : ISearchService
    {
        #region Fields

        private readonly Assembly _assembly;
        private IEnumerable<QueryResult> _reservedAliases = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Generate a new instance. Look into the Executing Assembly to find reserved aliases.
        /// </summary>
        /// <remarks>
        /// Each reserved alias should be decarated with <see cref="ReservedAliasAttribute"/>
        /// </remarks>
        public ReservedAliasStore()
        {
            _assembly = Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// Generate a new instance
        /// </summary>
        /// <param name="assembly">The assembly where to search the reseved aliases. </param>
        /// <remarks>
        /// Each reserved alias should be decorated with <see cref="ReservedAliasAttribute"/>
        /// </remarks>
        public ReservedAliasStore(Assembly assembly)
        {
            _assembly = assembly;
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<QueryResult> ReservedAliases
        {
            get
            {
                if (_reservedAliases == null) { LoadAliases(); }
                return _reservedAliases;
            }
        }

        #endregion Properties

        #region Methods

        private void LoadAliases()
        {
            if (_reservedAliases == null)
            {
                var types = _assembly.GetTypes();

                var found = (from t in types
                             where t.GetCustomAttributes<ReservedAliasAttribute>().Any()
                             select t).ToList();
                var stores = new List<QueryResult>();
                foreach (var type in found)
                {
                    var instance = Activator.CreateInstance(type);

                    if (instance is ExecutableQueryResult qr)
                    {
                        qr.Name = (type.GetCustomAttribute(typeof(ReservedAliasAttribute)) as ReservedAliasAttribute)?.Name;
                        qr.SetDescription((type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description);
                        qr.Icon = "keylink";

                        stores.Add(qr);
                    }
                }

                _reservedAliases = stores;
            }
        }

        public IEnumerable<QueryResult> GetAll() => ReservedAliases;

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            return (from k in ReservedAliases
                    where k.Name.ToLower().StartsWith(query.Name)
                    select k).ToList();
        }

        #endregion Methods
    }
}