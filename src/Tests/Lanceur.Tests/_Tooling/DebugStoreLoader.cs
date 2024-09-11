using System.Reflection;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Stores;
using NSubstitute;

namespace Lanceur.Tests.Tooling;

internal class DebugStoreLoader : IStoreLoader
{
    #region Methods

    public IEnumerable<IStorehService> Load()
    {
        var results = new List<IStorehService> { new ReservedAliasStore(Assembly.GetExecutingAssembly(), Substitute.For<IDbRepository>()) };
        return results;
    }

    #endregion Methods
}