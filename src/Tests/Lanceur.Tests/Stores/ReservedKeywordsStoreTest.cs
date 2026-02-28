using System.Web.Bookmarks;
using Lanceur.Core;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tooling.ReservedAliases;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.ReservedAliases;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Stores;

public class ReservedKeywordsStoreTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public ReservedKeywordsStoreTest(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    private ReservedAliasStore GetStore(IAliasRepository aliasRepository, Type type = null)
    {
        type ??= typeof(NotExecutableTestAlias);

        var serviceProvider = new ServiceCollection()
                              .AddSingleton(new AssemblySource { ReservedKeywordSource = type.Assembly })
                              .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                              .AddSingleton(Substitute.For<IApplicationSettingsProvider>())
                              .AddSingleton(aliasRepository)
                              .AddTestOutputHelper(_output)
                              .AddLoggingForTests(_output)
                              .AddMockSingleton<IBookmarkRepositoryFactory>()
                              .AddMockSingleton<IConfigurationFacade>((_, i) =>
                                  {
                                      i.Application.Returns(new ApplicationSettings());
                                      return i;
                                  }
                              )
                              .AddSingleton<ReservedAliasStore>()
                              .AddReservedAliasesServices(type)
                              .AddMockSingleton<IUserDialogueService>()
                              .AddMockSingleton<IUserNotificationService>()
                              .BuildServiceProvider();

        var store = serviceProvider.GetService<ReservedAliasStore>();
        return store;
    }

    [Fact]
    public void When_search_found_alias_Then_it_has_correct_count_value()
    {
        const int count = 100;
        const int id = 12;

        var aliasRepository = Substitute.For<IAliasRepository>();
        aliasRepository.GetHiddenCounters()
                       .Returns(new Dictionary<string, (long, int)> { { Names.Name1, (id, count) } });

        var store = GetStore(aliasRepository, typeof(ExecutableTestAlias));

        var result = store.Search(Cmdline.Parse(Names.Name1))
                          .ToArray();

        result.ShouldSatisfyAllConditions(
            r => r.Length.ShouldBe(1),
            r => r[0].Count.ShouldBe(count),
            r => r[0].Id.ShouldBe(id)
        );
    }

    [Theory]
    [InlineData("add")]
    [InlineData("quit")]
    [InlineData("setup")]
    [InlineData("version")]
    [InlineData("clrbm")]
    [InlineData("logs")]
    public void When_search_Then_ReservedAlias_exists_in_store_by_default(string criterion)
    {
        var repository = Substitute.For<IAliasRepository>();
        var store = GetStore(repository, typeof(MainView));
        var query = new Cmdline(criterion);

        store.Search(query).Count().ShouldBe(1);
    }

    #endregion
}