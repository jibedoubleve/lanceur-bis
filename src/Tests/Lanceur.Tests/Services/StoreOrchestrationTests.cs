using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Lanceur.Tests.Tools.Extensions;
using Lanceur.Ui.WPF.Converters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class StoreOrchestrationTests
{
    #region Methods

    [Theory]
    [InlineData("m", ".", false)]
    [InlineData("dm", "m", false)]
    [InlineData(".", ".", true)]
    [InlineData("m", "m", true)]
    [InlineData("mm", "m", true)]
    [InlineData("    mm", "m", true)]
    [InlineData("/", "/", true)]
    [InlineData(".somest", ".", true)]
    //-- With params
    [InlineData("m --with parameters", ".", false)]
    [InlineData("dm --with parameters", "m", false)]
    [InlineData(". --with parameters", ".", true)]
    [InlineData("m --with parameters", "m", true)]
    [InlineData("mm --with parameters", "m", true)]
    [InlineData("    mm             --with parameters", "m", true)]
    [InlineData("/ --with parameters", "/", true)]
    [InlineData("/--with parameters", "/", true)]
    public void When_checking_store_is_alive_Then_it_is_alive_or_not_as_expected(
        string cmd, string regex, bool expected)
    {
        // arrange
        new ServiceCollection()
            .AddLogging(builder => builder.AddXUnit())
            .AddMockConfigurationSections()
            .BuildServiceProvider();
        var storeService = Substitute.For<IStoreService>();
        storeService.Orchestration.Returns(new StoreOrchestrationFactory().Exclusive(regex));

        // act
        storeService.Orchestration.IsAlive(Cmdline.Parse(cmd))
                    .ShouldBe(expected);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("")]
    [InlineData("m")]
    public void When_convert_back_value_Then_same_value_returned(string input)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.ConvertBack(input,
                     null!,
                     null,
                     null!)
                 .ShouldBe(input);
    }

    [Theory]
    [InlineData(@"^\s{0,}\..*", ".")]
    [InlineData(@"^\s{0,}.*", "")]
    [InlineData(@"^\s{0,}m.*", "m")]
    public void When_converting_old_regex_Then_shortcut_is_returned_for_retro_compatibility(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.Convert(input,
                     null!,
                     null,
                     null!)
                 .ShouldBe(output);
    }
    
    [Fact]
    public void When_store_is_always_inactive_but_shortcut_override_exists_in_settings_Then_store_should_be_inactive()
    {
        // arrange
        var featureFlags = Substitute.For<IFeatureFlagService>();
        featureFlags.IsEnabled(Features.SteamIntegration).Returns(false); // feature flag OFF

        var store = new SteamGameStore(
            new StoreOrchestrationFactory(),
            new StaticSection(new StoreSection()),
            Substitute.For<ISteamLibraryService>(),
            Substitute.For<IAliasManagementService>(),
            featureFlags
        );

        // act — query starts with "&", which matches the override pattern
        var result = store.Orchestration.IsAlive(Cmdline.Parse("&steam"));

        // assert — AlwaysInactive must win: the disabled feature flag takes precedence over any stored shortcut
        result.ShouldBeFalse();
    }

    #endregion

    // Wraps a StoreSection value without any persistence logic
    private sealed class StaticSection(StoreSection value) : ISection<StoreSection>
    {
        #region Properties

        public StoreSection Value => value;

        #endregion

        #region Methods

        public void Reload() { }

        #endregion
    }
}