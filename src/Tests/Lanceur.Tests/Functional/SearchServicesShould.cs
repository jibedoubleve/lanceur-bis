using System.Reflection;
using System.Text.RegularExpressions;
using Everything.Wrapper;
using FluentAssertions;
using Lanceur.Core;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Functional;

public class SearchServicesShould
{
    #region Properties

    private IDbRepository DbRepository => Substitute.For<IDbRepository>();

    private ILoggerFactory LoggerFactory => Substitute.For<ILoggerFactory>();

    #endregion Properties

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
                                                     .AddSingleton(DbRepository)
                                                     .BuildServiceProvider();
        var store = new AdditionalParametersStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeTrue();
    }

    [Theory]
    [InlineData("6+5")]
    [InlineData("(4+4)+5")]
    [InlineData("45 + (689)")]
    [InlineData("(")]
    [InlineData("45  + (")]
    [InlineData(" 45  + (")]
    [InlineData("0")]
    [InlineData(" 0")]
    public void ActivateCalculatorStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var store = new CalculatorStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeTrue();
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
                                                     .AddSingleton(LoggerFactory)
                                                     .BuildServiceProvider();
        var store = new EverythingStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeTrue();
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
                                                     .AddSingleton(DbRepository)
                                                     .BuildServiceProvider();
        var store = new AdditionalParametersStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeFalse();
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
        var serviceProvider = new ServiceCollection().AddSingleton(DbRepository)
                                                     .AddSingleton(LoggerFactory)
                                                     .BuildServiceProvider();
        var store = new AliasStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeTrue();
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
                                                     .AddSingleton(DbRepository)
                                                     .AddSingleton(LoggerFactory)
                                                     .BuildServiceProvider();
        var store = new ReservedAliasStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(":1254")]
    [InlineData(">undeux")]
    [InlineData("sqlite")]
    [InlineData("123")]
    [InlineData("----")]
    public void ActivatePluginStore(string query)
    {
        // ACT
        var serviceProvider = new ServiceCollection().AddSingleton(DbRepository)
                                                     .AddSingleton(LoggerFactory)
                                                     .AddSingleton(Substitute.For<IPluginStoreContext>())
                                                     .AddSingleton(Substitute.For<IPluginManager>())
                                                     .BuildServiceProvider();
        var store = new PluginStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeTrue();
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
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var store = new CalculatorStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeFalse();
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
        var serviceProvider = new ServiceCollection().AddSingleton(DbRepository)
                                                     .AddSingleton(Substitute.For<IEverythingApi>())
                                                     .BuildServiceProvider();
        var store = new EverythingStore(serviceProvider);

        // ASSERT
        var regex = new Regex(store.Orchestration.AlivePattern);
        regex.IsMatch(query).Should().BeFalse();
    }

    #endregion Methods
}