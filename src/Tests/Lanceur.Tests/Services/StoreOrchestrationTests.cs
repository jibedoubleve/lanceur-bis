using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores;
using Lanceur.Ui.WPF.Converters;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class StoreOrchestrationTests
{
    #region Methods

    /// <summary>
    ///     An exclusive store wakes up when the name of the query is strictly equal to its
    ///     shortcut. Whatever follows the name is a parameter, glued or not.
    /// </summary>
    [Theory]
    [InlineData(".", ".", true)]
    [InlineData("   .", ".", true)]
    [InlineData(".somest", ".", true)]
    [InlineData("m", ".", false)]
    [InlineData("/", "/", true)]
    [InlineData("&", "&", true)]
    //-- A repeated special character is another name, it must not wake the store up
    [InlineData("&&", "&", false)]
    [InlineData("&&foo", "&", false)]
    [InlineData("?? foo", "?", false)]
    [InlineData("&a&", "&", true)]
    //-- With params
    [InlineData(". --with parameters", ".", true)]
    [InlineData("m --with parameters", ".", false)]
    [InlineData("/ --with parameters", "/", true)]
    [InlineData("/--with parameters", "/", true)]
    [InlineData("& game", "&", true)]
    public void When_checking_store_is_alive_Then_it_is_alive_or_not_as_expected(
        string cmd, string shortcut, bool expected)
        => new StoreOrchestrationFactory().Exclusive(shortcut)
                                          .IsAlive(Cmdline.Parse(cmd))
                                          .ShouldBe(expected);

    [Theory]
    [InlineData("&", true)]
    [InlineData(":", true)]
    [InlineData("&&", false)]
    [InlineData("st", false)]
    [InlineData("s", false)]
    [InlineData("é", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void When_checking_store_shortcut_Then_only_a_single_special_character_is_valid(
        string? shortcut, bool expected)
        => CmdlineManager.IsSpecialCharacter(shortcut).ShouldBe(expected);

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

        // act - query starts with "&", which matches the override pattern
        var result = store.Orchestration.IsAlive(Cmdline.Parse("&steam"));

        // assert - AlwaysInactive must win: the disabled feature flag takes precedence over any stored shortcut
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