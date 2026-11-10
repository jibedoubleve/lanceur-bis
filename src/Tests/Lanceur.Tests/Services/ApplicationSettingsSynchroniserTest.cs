using System.Text.Json;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Stores.Everything;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

/// <remarks>
///     The view model saves the settings each time one of its properties changes. To test
///     <see cref="ApplicationSettingsSynchroniser.SaveSettings" /> without this side effect, these tests
///     change the stored settings rather than the view model to make both diverge.
/// </remarks>
public sealed class ApplicationSettingsSynchroniserTest
{
    #region Fields

    private const string EncryptionPrefix = "encrypted:";

    #endregion

    #region Methods

    private static string Encrypted(string value) => $"{EncryptionPrefix}{value}";

    private static Fixture CreateFixture(Action<ApplicationSettings>? appSettingsHook = null)
    {
        var appSettings = new ApplicationSettings();
        appSettingsHook?.Invoke(appSettings);

        var app = Substitute.For<ISettingsProvider<ApplicationSettings>>();
        app.Value.Returns(appSettings);

        var infra = Substitute.For<ISettingsProvider<InfrastructureSettings>>();
        infra.Value.Returns(new InfrastructureSettings());

        var storeShortcutService = Substitute.For<IStoreShortcutService>();
        storeShortcutService.Resolve(Arg.Any<StoreSection>())
                            .Returns(ci => ci.Arg<StoreSection>().StoreShortcuts);

        var enigma = Substitute.For<IEnigma>();
        enigma.Encrypt(Arg.Any<string>()).Returns(ci => Encrypted(ci.Arg<string>()));
        enigma.Decrypt(Arg.Any<string>()).Returns(ci => ci.Arg<string>()[EncryptionPrefix.Length..]);

        var sut = new ApplicationSettingsSynchroniser(
            app,
            infra,
            storeShortcutService,
            enigma,
            NullLogger<ApplicationSettingsSynchroniser>.Instance
        );

        var vm = new ApplicationSettingsViewModel(
            Substitute.For<IUserCommunicationService>(),
            NullLogger<ApplicationSettingsViewModel>.Instance,
            Substitute.For<IViewFactory>(),
            sut,
            Substitute.For<IAliasRepository>()
        );

        app.ClearReceivedCalls();
        infra.ClearReceivedCalls();
        enigma.ClearReceivedCalls();

        return new(sut, vm, app, infra, enigma);
    }

    private static string Snapshot(object settings) => JsonSerializer.Serialize(settings);

    // --- SaveSettings: restart detection ---

    [Fact]
    public void When_nothing_changes_Then_no_restart_is_needed()
    {
        var f = CreateFixture();

        f.Sut.SaveSettings(f.ViewModel).ShouldBeFalse();
    }

    [Fact]
    public void When_hotkey_changes_Then_restart_is_needed()
    {
        var f = CreateFixture();
        f.AppSettings.Value.HotKey = new(8, 42);

        f.Sut.SaveSettings(f.ViewModel).ShouldBeTrue();
    }

    [Fact]
    public void When_hotkey_is_another_instance_with_same_values_Then_no_restart_is_needed()
    {
        var f = CreateFixture();
        var current = f.AppSettings.Value.HotKey;
        f.AppSettings.Value.HotKey = new(current.ModifierKey, current.Key);

        f.Sut.SaveSettings(f.ViewModel).ShouldBeFalse();
    }

    [Fact]
    public void When_db_path_changes_Then_restart_is_needed()
    {
        var f = CreateFixture();
        f.InfraSettings.Value.Database.DbPath = "other.sqlite";

        f.Sut.SaveSettings(f.ViewModel).ShouldBeTrue();
    }

    [Fact]
    public void When_log_level_changes_Then_restart_is_needed()
    {
        var f = CreateFixture();
        f.InfraSettings.Value.Logging.MinimumLogLevel = f.ViewModel.SelectedLogLevel == LogLevel.Trace
            ? LogLevel.Information
            : LogLevel.Trace;

        f.Sut.SaveSettings(f.ViewModel).ShouldBeTrue();
    }

    [Fact]
    public void When_a_setting_without_restart_changes_Then_no_restart_is_needed_and_it_is_saved()
    {
        var f = CreateFixture();
        var expected = f.ViewModel.SearchDelay;
        f.AppSettings.Value.SearchBox.SearchDelay = expected + 100;

        f.Sut.SaveSettings(f.ViewModel).ShouldBeFalse();
        f.AppSettings.Value.SearchBox.SearchDelay.ShouldBe(expected);
    }

    [Fact]
    public void When_saving_Then_both_settings_are_persisted_once()
    {
        var f = CreateFixture();

        f.Sut.SaveSettings(f.ViewModel);

        f.AppSettings.Received(1).Save();
        f.InfraSettings.Received(1).Save();
    }

    // --- Autosave during mapping ---

    [Fact]
    public void When_mapping_from_settings_Then_nothing_is_saved()
    {
        var f = CreateFixture();
        f.AppSettings.Value.SearchBox.SearchDelay = f.ViewModel.SearchDelay + 100;
        f.AppSettings.Value.Window.DateTimeFormat = "yyyy";
        f.InfraSettings.Value.Database.DbPath = "other.sqlite";

        f.Sut.MapFrom(f.ViewModel);

        f.AppSettings.DidNotReceive().Save();
        f.InfraSettings.DidNotReceive().Save();
    }

    [Fact]
    public void When_mapping_is_done_Then_autosave_is_enabled_again()
    {
        var f = CreateFixture();
        f.Sut.MapFrom(f.ViewModel);

        f.ViewModel.SearchDelay += 100;

        f.AppSettings.Received(1).Save();
    }

    // --- Round trip ---

    [Fact]
    public void When_settings_are_loaded_then_saved_without_change_Then_they_are_unchanged()
    {
        // A null token is saved as an empty string, which is equivalent
        var f = CreateFixture(s => s.Github.Token = string.Empty);
        var appBefore = Snapshot(f.AppSettings.Value);
        var infraBefore = Snapshot(f.InfraSettings.Value);

        f.Sut.MapFrom(f.ViewModel);
        var needRestart = f.Sut.SaveSettings(f.ViewModel);

        needRestart.ShouldBeFalse();
        Snapshot(f.AppSettings.Value).ShouldBe(appBefore);
        Snapshot(f.InfraSettings.Value).ShouldBe(infraBefore);
    }

    [Theory]
    [InlineData("")]
    [InlineData($"!{EverythingModifiers.IncludeHiddenFilesSwitch}")]
    [InlineData($"!{EverythingModifiers.IncludeSystemFilesSwitch}")]
    [InlineData(EverythingModifiers.OnlyExecFilesSwitch)]
    [InlineData(EverythingModifiers.ExcludeFileInTrashBin)]
    [InlineData(
        $"!{EverythingModifiers.IncludeHiddenFilesSwitch} !{EverythingModifiers.IncludeSystemFilesSwitch} {EverythingModifiers.OnlyExecFilesSwitch} {EverythingModifiers.ExcludeFileInTrashBin}"
    )]
    public void When_everything_query_is_loaded_then_saved_Then_it_is_unchanged(string suffix)
    {
        var f = CreateFixture(s => s.Stores.EverythingQuerySuffix = suffix);

        f.Sut.MapFrom(f.ViewModel);
        f.Sut.SaveSettings(f.ViewModel);

        f.AppSettings.Value.Stores.EverythingQuerySuffix.ShouldBe(suffix);
    }

    // --- Feature flags ---

    [Fact]
    public void When_mapping_from_settings_Then_view_model_has_a_copy_of_the_feature_flags()
    {
        var f = CreateFixture();
        f.ViewModel.FeatureFlags = [];

        f.Sut.MapFrom(f.ViewModel);

        f.ViewModel.FeatureFlags.ShouldNotBeSameAs(f.AppSettings.Value.FeatureFlags);
        f.ViewModel.FeatureFlags.ShouldBe(f.AppSettings.Value.FeatureFlags);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void When_feature_flag_is_toggled_in_view_model_Then_it_is_saved(bool enabled)
    {
        var f = CreateFixture(s => s.FeatureFlags.Single(x => x.FeatureName == Features.ResourceDisplay)
                                    .Enabled = !enabled
        );

        f.ViewModel.FeatureFlags.Single(x => x.FeatureName == Features.ResourceDisplay).Enabled = enabled;
        f.Sut.SaveSettings(f.ViewModel);

        f.AppSettings.Value.FeatureFlags
         .Single(x => x.FeatureName == Features.ResourceDisplay)
         .Enabled.ShouldBe(enabled);
        f.ViewModel.IsResourceMonitorEnabled.ShouldBe(enabled);
    }

    // --- GitHub token ---

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void When_stored_token_is_empty_Then_view_model_token_is_empty_and_not_decrypted(string? token)
    {
        var f = CreateFixture(s => s.Github.Token = token);

        f.Sut.MapFrom(f.ViewModel);

        f.ViewModel.ApiToken.ShouldBeEmpty();
        f.Enigma.DidNotReceive().Decrypt(Arg.Any<string>());
    }

    [Fact]
    public void When_stored_token_is_set_Then_view_model_token_is_decrypted()
    {
        var f = CreateFixture(s => s.Github.Token = Encrypted("secret"));

        f.Sut.MapFrom(f.ViewModel);

        f.ViewModel.ApiToken.ShouldBe("secret");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void When_view_model_token_is_empty_Then_stored_token_is_empty_and_not_encrypted(string token)
    {
        var f = CreateFixture(s => s.Github.Token = Encrypted("secret"));
        f.ViewModel.ApiToken = token;
        f.Enigma.ClearReceivedCalls();

        f.Sut.SaveSettings(f.ViewModel);

        f.AppSettings.Value.Github.Token.ShouldBeEmpty();
        f.Enigma.DidNotReceive().Encrypt(Arg.Any<string>());
    }

    [Fact]
    public void When_view_model_token_is_set_Then_stored_token_is_encrypted()
    {
        var f = CreateFixture();
        f.ViewModel.ApiToken = "secret";

        f.Sut.SaveSettings(f.ViewModel);

        f.AppSettings.Value.Github.Token.ShouldBe(Encrypted("secret"));
    }

    #endregion

    #region Nested type: Fixture

    private sealed record Fixture(
        ApplicationSettingsSynchroniser Sut,
        ApplicationSettingsViewModel ViewModel,
        ISettingsProvider<ApplicationSettings> AppSettings,
        ISettingsProvider<InfrastructureSettings> InfraSettings,
        IEnigma Enigma
    );

    #endregion
}
