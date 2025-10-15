using Shouldly;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Ui.WPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Services;

public class SearchServiceOrchestratorShould
{
    #region Methods

    [Theory]
    [InlineData(".", @"^\s{0,}\..*")]
    [InlineData("", @"^\s{0,}.*")]
    [InlineData("m", @"^\s{0,}m.*")]
    public void ConvertBackDotAsValueNotOperator(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.ConvertBack(input, null!, null, null!)
                 .Should()
                 .Be(output);
    }
    
    [Theory]
    [InlineData(@"^\s{0,}\..*", ".")]
    [InlineData(@"^\s{0,}.*", "")]
    [InlineData(@"^\s{0,}m.*", "m")]
    public void ConvertDotAsValueNotOperator(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.Convert(input, null!, null, null!)
                 .Should()
                 .Be(output);
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
                 .AddMockSingleton<ISettingsFacade>(
                     (_, i) =>
                     {
                         i.Application.Returns(new DatabaseConfiguration());
                         return i;
                     }
                 )
                 .BuildServiceProvider();
        var storeService = Substitute.For<IStoreService>();
        storeService.StoreOrchestration.Returns(new StoreOrchestrationFactory().Exclusive(regex));

        var orchestrator = new SearchServiceOrchestrator(sp.GetService<ILoggerFactory>(), sp.GetService<ISettingsFacade>());

        // act
        orchestrator.IsAlive(storeService, Cmdline.Parse(cmd))
                    .Should()
                    .Be(expected);
    }

    #endregion
}