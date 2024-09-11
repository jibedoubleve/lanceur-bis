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

    public IEnumerable<IStoreService> Load()
    {
        var results = new List<IStoreService> { new ReservedAliasStore(Assembly.GetExecutingAssembly(), Substitute.For<IDbRepository>()) };
        return results;
    }

    #endregion Methods
}