using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Stores;
using NSubstitute;
using System.Reflection;
using Lanceur.Core.Repositories.Config;
using Lanceur.Tests.Tooling.Extensions;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Lanceur.Tests.BusinessLogic;

public class InternalAliasStoreShould
{
    #region Methods

    private static ReservedAliasStore GetStore(IDbRepository dbRepository, Type type = null)
    {
        type ??= typeof(NotExecutableTestAlias);
        var serviceProvider = new ServiceCollection().AddSingleton(Assembly.GetAssembly(type)!)
                                                     .AddSingleton<ILoggerFactory, LoggerFactory>()
                                                     .AddSingleton(Substitute.For<IAppConfigRepository>())
                                                     .AddSingleton(dbRepository)
                                                     .BuildServiceProvider();

        var store = new ReservedAliasStore(serviceProvider);
        return store;
    }

    [Theory]
    [InlineData("add")]
    [InlineData("quit")]
    [InlineData("setup")]
    [InlineData("version")]
    public void ReturnSpecifiedReservedAliasFromLanceur(string criterion)
    {
        var repository = Substitute.For<IDbRepository>();
        repository.RefreshUsage(Arg.Any<IEnumerable<QueryResult>>())
                  .ReturnsForAnyArgs(x => x.Args()[0] as IEnumerable<QueryResult>);

        var store = GetStore(repository, typeof(MainView));
        var query = new Cmdline(criterion);

        store.Search(query)
             .Should()
             .HaveCount(1);
    }

    [Fact]
    public void ReturnSpecifiedReservedKeyword()
    {
        var serviceProvider  = new ServiceCollection().AddMockSingleton<IDbRepository>(
                                                          (sp, repository) =>
                                                          {
                                                              repository.RefreshUsage(Arg.Any<IEnumerable<QueryResult>>())
                                                                        .Returns((List<QueryResult>) [new ExecutableTestAlias(sp)]);
                                                              return repository;
                                                          }
                                                      )
                                                      .BuildServiceProvider();

        var store = GetStore(serviceProvider.GetService<IDbRepository>());
        var query = new Cmdline("anothertest");

        store.Search(query)
             .Should()
             .HaveCount(1);
    }

    #endregion
}