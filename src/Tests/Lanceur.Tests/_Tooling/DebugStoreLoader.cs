using System.Reflection;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Stores;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lanceur.Tests.Tooling;

internal class DebugStoreLoader : IStoreLoader
{
    #region Methods

    public IEnumerable<IStoreService> Load()
    {
        var serviceProvider = new ServiceCollection().AddSingleton(Assembly.GetExecutingAssembly())
                                                     .AddSingleton(Substitute.For<IAliasRepository>())
                                                     .BuildServiceProvider();
        return new List<IStoreService> { new ReservedAliasStore(serviceProvider) };
    }

    #endregion Methods
}