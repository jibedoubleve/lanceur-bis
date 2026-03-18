using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Tests.Tools.Helpers;
using Lanceur.Ui.WPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class SearchServiceOrchestratorShould
{
    #region Methods

    [Theory]
    [InlineData(".", @"^\s{0,}\..*")]
    [InlineData("", @"^\s{0,}.*")]
    [InlineData("m", @"^\s{0,}m.*")]
    public void ConvertBackDotAsValueNotOperator(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.ConvertBack(
                     input,
                     null!,
                     null,
                     null!
                 )
                 .ShouldBe(output);
    }

    [Theory]
    [InlineData(@"^\s{0,}\..*", ".")]
    [InlineData(@"^\s{0,}.*", "")]
    [InlineData(@"^\s{0,}m.*", "m")]
    public void ConvertDotAsValueNotOperator(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.Convert(
                     input,
                     null!,
                     null,
                     null!
                 )
                 .ShouldBe(output);
    }

    [Theory]
    [InlineData("m", "^\\s{0,}\\..*", false)]
    [InlineData(".", "^\\s{0,}\\..*", true)]
    [InlineData("m", "^\\s{0,}m.*", true)]
    [InlineData("mm", "^\\s{0,}m.*", true)]
    [InlineData("    mm", "^\\s{0,}m.*", true)]
    [InlineData("dm", "^\\s{0,}m.*", false)]
    [InlineData("/", "^\\s{0,}/.*", true)]
    //--
    [InlineData(".somest", "^\\s{0,}\\..*", true)]
    public void SelectExpectedCmdlines(string cmd, string regex, bool expected)
    {
        // arrange
        var sp = new ServiceCollection()
                 .AddLogging(builder => builder.AddXUnit())
                 .AddMockConfigurationSections()
                 .BuildServiceProvider();
        var storeService = Substitute.For<IStoreService>();
        storeService.StoreOrchestration.Returns(new StoreOrchestrationFactory().Exclusive(regex));

        var orchestrator = new SearchServiceOrchestrator(
            sp.GetSection<StoreSection>()!
        );

        // act
        orchestrator.IsAlive(storeService, Cmdline.Parse(cmd))
                    .ShouldBe(expected);
    }

    #endregion
}