using System.Text.RegularExpressions;
using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Repositories;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Tests.Tooling;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Functional;

public class SettingsMementoManagerShould
{
    #region Fields

    private readonly ITestOutputHelper _output;

    /// <remarks>
    ///     Spacing is very important. If you change this value,
    ///     Check the regular expression to be sure it'll work
    ///     as expected.
    /// </remarks>
    private const string JsonAppConfig = """
                                         {
                                             "HotKey": {
                                                 "Key": 18,
                                                 "ModifierKey": 3
                                             },
                                             "Repository": {
                                                 "ScoreLimit": 0
                                             },
                                             "RestartDelay": 500.0,
                                             "Window": {
                                                 "Position": {
                                                     "Left": 2200.0,
                                                     "Top": 200.0
                                                 },
                                                 "ShowAtStartup": true,
                                                 "ShowResult": true
                                             }
                                         }
                                         """;

    #endregion

    #region Constructors

    public SettingsMementoManagerShould(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    [Theory]
    [InlineData("Key", "100", "", true)]
    [InlineData("ModifierKey", "100", "", true)]
    [InlineData("", "", "newDbPath", true)]
    [InlineData("ScoreLimit", "100", "", false)]
    [InlineData("RestartDelay", "100", "", false)]
    [InlineData("Left", "100.0", "", false)]
    [InlineData("Top", "100.0", "", false)]
    [InlineData("ShowAtStartup", "false", "", false)]
    [InlineData("ShowResult", "false", "", false)]
    public void ReturnExpectedStateChange(string property, string newValue, string secondDbPath, bool isStateChanged)
    {
        // ARRANGE
        var pattern = $"""
                       ("{property}": [a-zA-Z0-9.]*)
                       """;
        var regex = new Regex(pattern, RegexOptions.Singleline);
        var dbPath = Guid.NewGuid().ToString();

        // Initial state
        var initialAppConfig = JsonConvert.DeserializeObject<DatabaseConfiguration>(JsonAppConfig);
        _output.WriteJson("Initial AppConfig", initialAppConfig);

        // Second state
        var replacement = $"""
                           "{property}": {newValue}
                           """;
        var newJson = regex.Replace(JsonAppConfig, replacement);
        var secondAppConfig = JsonConvert.DeserializeObject<DatabaseConfiguration>(newJson);
        _output.WriteJson("Second AppConfig", secondAppConfig);
        _output.WriteLine($"Replacement: '{replacement}', Pattern: '{pattern}'");

        secondDbPath = secondDbPath.IsNullOrWhiteSpace() ? dbPath : secondDbPath;

        // Setup SettingsFacade
        var databaseConfig = Substitute.For<IApplicationSettings>();
        databaseConfig.DbPath.Returns(dbPath, secondDbPath);

        var databaseConfigRepository = Substitute.For<IApplicationConfigurationService>();
        databaseConfigRepository.Current.Returns(databaseConfig);

        var appConfigRepository = Substitute.For<IDatabaseConfigurationService>();
        appConfigRepository.Current.Returns(initialAppConfig, secondAppConfig);

        var settingsFacade = new SettingsFacadeService(databaseConfigRepository, appConfigRepository);

        // ACT
        var memento = SettingsMementoService.GetInitialState(settingsFacade);

        // ASSERT
        memento.HasStateChanged(settingsFacade)
               .Should()
               .Be(isStateChanged);
    }

    [Fact]
    public void ReturnsNotChangedWhenNoChange()
    {
        // ARRANGE
        // First state
        var dbPath = Guid.NewGuid().ToString();
        var initialAppConfig = JsonConvert.DeserializeObject<DatabaseConfiguration>(JsonAppConfig);

        // Second state
        var secondAppConfig = JsonConvert.DeserializeObject<DatabaseConfiguration>(JsonAppConfig);
        var secondDbPath = dbPath;

        // Setup SettingsFacade
        var databaseConfig = Substitute.For<IApplicationSettings>();
        databaseConfig.DbPath.Returns(dbPath, secondDbPath);

        var databaseConfigRepository = Substitute.For<IApplicationConfigurationService>();
        databaseConfigRepository.Current.Returns(databaseConfig);

        var appConfigRepository = Substitute.For<IDatabaseConfigurationService>();
        appConfigRepository.Current.Returns(initialAppConfig, secondAppConfig);

        var settingsFacade = new SettingsFacadeService(databaseConfigRepository, appConfigRepository);

        // ACT
        var memento = SettingsMementoService.GetInitialState(settingsFacade);

        // ASSERT
        memento.HasStateChanged(settingsFacade).Should().BeFalse();
    }

    #endregion
}