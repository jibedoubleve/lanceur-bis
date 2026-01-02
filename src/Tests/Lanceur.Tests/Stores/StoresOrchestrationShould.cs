using System.Text.RegularExpressions;
using Everything.Wrapper;
using Shouldly;
using Lanceur.Core;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Logging;
using Lanceur.Ui.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Stores;

public class StoresOrchestrationShould
{
    #region Fields

    private readonly ITestOutputHelper _outputHelper;

    #endregion

    #region Constructors

    public StoresOrchestrationShould(ITestOutputHelper outputHelper) => _outputHelper = outputHelper;

    #endregion

    #region Properties

    private static IAliasRepository AliasRepository => Substitute.For<IAliasRepository>();

    private ILoggerFactory LoggerFactory => new MicrosoftLoggingLoggerFactory(_outputHelper);

    #endregion

    #region Methods

    [Theory]
    [InlineData("r:mlk")]
    [InlineData("r:")]
    [InlineData("r:465")]
    [InlineData("r:::::")]
    [InlineData("r:mlk mlk")]
    [InlineData("r: mlk")]
    [InlineData("r:465 mlk")]
    [InlineData("r::::: mlk")]
    public void ActivateAdditionalParametersStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton(LoggerFactory)
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton(AliasRepository)
                                                     .AddSingleton<AdditionalParametersStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .AddStoreServicesMockContext()
                                                     .AddStoreServices()
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<AdditionalParametersStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(":1254")]
    [InlineData(">undeux")]
    [InlineData("sqlite")]
    [InlineData("123")]
    [InlineData("----")]
    public void ActivateAliasStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton(AliasRepository)
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton(LoggerFactory)
                                                     .AddSingleton<AliasStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .AddStoreServicesMockContext()
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<AliasStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeTrue();
    }

    [Theory]
    [InlineData("6+5")]
    [InlineData("(4+4)+5")]
    [InlineData("45 + (689)")]
    [InlineData("( 10 + 10")]
    [InlineData("45  + (")]
    [InlineData(" 45  + (")]
    [InlineData("0")]
    [InlineData(" 0")]
    public void ActivateCalculatorStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                                                     .AddSingleton<IStoreService, CalculatorStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<IStoreService>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeTrue();
    }
    [Theory]
    [InlineData("6+5", "11")]
    [InlineData("(6+5)+1", "12")]
    public async Task UnderstandCalculationWithOrchestration(string query, string expected)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddConfigurationSections()
          .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
          .AddSingleton<AliasStore>()
          .AddLogging()
          .AddSingleton<CalculatorStore>()
          .AddSingleton<ICalculatorService, NCalcCalculatorService>()
          .AddSingleton<ISearchService, SearchService>()
          .AddSingleton<ISearchServiceOrchestrator, SearchServiceOrchestrator>()
          .AddMockSingleton<IThumbnailService>()
          .AddLoggerFactoryForTests(_outputHelper)
          .AddMockSingleton<IMacroService>((_, i) =>
              {
                  i.ExpandMacroAlias(Arg.Any<QueryResult[]>())
                   .Returns(callInfo => callInfo.Arg<QueryResult[]>());
                  return i;
              }
          )
          .AddMockSingleton<IAliasRepository>((_, i) =>
              {
                  i.Search(Arg.Any<string>()).Returns([]);
                  return i;
              }
          )
          .AddMockSingleton<IConfigurationFacade>((_, i) =>
              {
                  i.Application.Returns(new ApplicationSettings());
                  return i;
              }
          );

        // Add stores...
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, AliasStore>());
        serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IStoreService, CalculatorStore>());
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var searchService = serviceProvider.GetService<ISearchService>();

        var result = await searchService.SearchAsync(Cmdline.Parse(query));
        result = result.ToArray();
        result.ShouldSatisfyAllConditions(
            r => r.Count().ShouldBe(1),
            r => r.ElementAt(0).Name.ShouldBe(expected)
        );
    }
    [Theory]
    [InlineData("1+1", "2")]
    [InlineData("2*3", "6")]
    [InlineData("2-3", "-1")]
    [InlineData("9/3", "3")]
    [InlineData("Sqrt(9)", "3")]
    [InlineData("sqrt(9)", "3")]
    [InlineData("(8+2) * 2 ", "20")]
    public void UseCalculatorAndReturnsExpectedResult(string query, string result)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                                                     .AddSingleton<IStoreService, CalculatorStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<IStoreService>();

            var regex = new Regex(store.StoreOrchestration.AlivePattern);
            var results = store.Search(Cmdline.Parse(query)).ToList();
            
            // ASSERT
            Assert.Multiple(
                () => regex.IsMatch(query).ShouldBeTrue(),
                () => results.ShouldSatisfyAllConditions(
                    r => r.Count.ShouldBeGreaterThan(0),
                    r => r.ElementAt(0).Name.ShouldBe(result)
                )
            );
    }

    [Theory]
    [InlineData(" : Hello")]
    [InlineData(": *.pdf")]
    [InlineData(":132")]
    [InlineData("::::")]
    [InlineData(":/.()}{")]
    [InlineData(":")]
    [InlineData(": ")]
    public void ActivateEverythingStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton(Substitute.For<IEverythingApi>())
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton(LoggerFactory)
                                                     .AddSingleton<EverythingStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .AddStoreServicesMockContext()
                                                     .AddStoreServicesConfiguration()
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<EverythingStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(":1254")]
    [InlineData(">undeux")]
    [InlineData("sqlite")]
    [InlineData("123")]
    [InlineData("----")]
    public void ActivateReservedAliasStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton<AssemblySource>()
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton(AliasRepository)
                                                     .AddSingleton(LoggerFactory)
                                                     .AddSingleton<ReservedAliasStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .AddStoreServicesMockContext()
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<ReservedAliasStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeTrue();
    }

    [Theory]
    [InlineData("sqlite")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("6+5")]
    [InlineData(">")]
    [InlineData(">dd")]
    [InlineData("69461")]
    [InlineData("sqlite mlk")]
    [InlineData("null mlk")]
    [InlineData("6+5 mlk")]
    [InlineData("> mlk")]
    [InlineData(">dd mlk")]
    [InlineData("69461 mlk")]
    public void DeactivateAdditionalParametersStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton(LoggerFactory)
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton(AliasRepository)
                                                     .AddSingleton<AdditionalParametersStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .AddStoreServicesMockContext()
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<AdditionalParametersStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeFalse();
    }


    [Theory]
    [InlineData("e6+5")]
    [InlineData("[4+4)+5")]
    [InlineData("\"45 + (689)")]
    [InlineData("'(")]
    [InlineData("g45  + (")]
    [InlineData(")0")]
    public void DeactivateCalculatorStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton<ICalculatorService, NCalcCalculatorService>()
                                                     .AddSingleton<CalculatorStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<CalculatorStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeFalse();
    }

    [Theory]
    [InlineData("undeux")]
    [InlineData("un :: :")]
    [InlineData("32131")]
    [InlineData("é'(§è(§é")]
    [InlineData("  undeux")]
    [InlineData("   g :: ")]
    public void DeactivateEverythingStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton(AliasRepository)
                                                     .AddSingleton<IStoreOrchestrationFactory>(new StoreOrchestrationFactory())
                                                     .AddSingleton(Substitute.For<IEverythingApi>())
                                                     .AddSingleton<EverythingStore>()
                                                     .AddTestOutputHelper(_outputHelper)
                                                     .AddStoreServicesMockContext()
                                                     .AddStoreServicesConfiguration()
                                                     .BuildServiceProvider();
        var store = serviceProvider.GetService<EverythingStore>();

        // ASSERT
        var regex = new Regex(store.StoreOrchestration.AlivePattern);
        regex.IsMatch(query).ShouldBeFalse();
    }

    #endregion
}