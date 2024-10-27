using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Stores;
using NSubstitute;
using System.Reflection;
using System.Windows.Documents.DocumentStructures;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Ui.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lanceur.Tests.BusinessLogic;

public class InternalAliasStoreShould
{
    #region Methods

    private static ReservedAliasStore GetStore(IDbRepository dbRepository, Type type = null)
    {
        type ??= typeof(NotExecutableTestAlias);
        var serviceProvider = new ServiceCollection().AddSingleton(Assembly.GetAssembly(type)!)
                                                     .AddSingleton(dbRepository)
                                                     .BuildServiceProvider();
        
        var store = new ReservedAliasStore(serviceProvider);
        return store;
    }

    [Theory, InlineData("add"), InlineData("quit"), InlineData("setup"), InlineData("version")]
    public void ReturnSpecifiedReservedAliasFromLanceur(string criterion)
    {
        var ds = Substitute.For<IDbRepository>();
        ds.RefreshUsage(Arg.Any<IEnumerable<QueryResult>>())
          .ReturnsForAnyArgs(x => x.Args()[0] as IEnumerable<QueryResult>);

        var store = GetStore(ds, typeof(MainViewModel));
        var query = new Cmdline(criterion);

        store.Search(query).Should().HaveCount(1);
    }

    [Fact]
    public void ReturnSpecifiedReservedKeyword()
    {
        var ds = Substitute.For<IDbRepository>();
        ds.RefreshUsage(Arg.Any<IEnumerable<QueryResult>>())
          .Returns(new List<QueryResult>() { new ExecutableTestAlias() });

        var store = GetStore(ds);
        var query = new Cmdline("anothertest");

        store.Search(query).Should().HaveCount(1);
        ;
    }

    #endregion Methods
}