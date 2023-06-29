using Lanceur.Core;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Splat;
using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Infra.Stores
{
    [Store]
    public class ReservedAliasStore : ISearchService
    {
        #region Fields

        private readonly Assembly _assembly;
        private readonly IDbRepository _dataService;
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
            _dataService = Locator.Current.GetService<IDbRepository>();
        }

        /// <summary>
        /// Generate a new instance
        /// </summary>
        /// <param name="assembly">The assembly where to search the reseved aliases. </param>
        /// <remarks>
        /// Each reserved alias should be decorated with <see cref="ReservedAliasAttribute"/>
        /// </remarks>
        public ReservedAliasStore(Assembly assembly) : this(assembly, null)
        {
        }

        /// <summary>
        /// Generate a new instance
        /// </summary>
        /// <param name="assembly">The assembly where to search the reseved aliases. </param>
        /// <param name="dataService">The dataservice used to update usage of the alias</param>
        /// <remarks>
        /// Each reserved alias should be decorated with <see cref="ReservedAliasAttribute"/>
        /// </remarks>
        public ReservedAliasStore(Assembly assembly, IDbRepository dataService)
        {
            _assembly = assembly;
            _dataService = dataService ?? Locator.Current.GetService<IDbRepository>();
        }

        #endregion Constructors

        #region Methods

        private void LoadAliases()
        {
            if (_reservedAliases == null)
            {
                var types = _assembly.GetTypes();

                var found = (from t in types
                             where t.GetCustomAttributes<ReservedAliasAttribute>().Any()
                             select t).ToList();
                var foundItems = new List<QueryResult>();
                foreach (var type in found)
                {
                    var instance = Activator.CreateInstance(type);

                    if (instance is SelfExecutableQueryResult qr)
                    {
                        var name = (type.GetCustomAttribute(typeof(ReservedAliasAttribute)) as ReservedAliasAttribute)?.Name; ;
                        var keyword = _dataService.GetKeyword(name);

                        qr.Name = name;
                        if (keyword is not null)
                        {
                            qr.Id = keyword.Id;
                            qr.Count = keyword.Count;
                        }

                        qr.Description = (type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
                        qr.Icon = "keylink";

                        foundItems.Add(qr);
                    }
                }

                _reservedAliases = foundItems;
            }
        }

        public IEnumerable<QueryResult> GetAll()
        {
            if (_reservedAliases == null) { LoadAliases(); }
            return _dataService.RefreshUsage(_reservedAliases);
        }

        public IEnumerable<QueryResult> Search(Cmdline query)
        {
            var result = (from k in GetAll()
                          where k.Name.ToLower().StartsWith(query.Name)
                          select k).ToList();

            var orderedResult = _dataService
                    .RefreshUsage(result)
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Name);
            return orderedResult;
        }

        #endregion Methods
    }
}