﻿using FluentAssertions;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Repositories;
using Lanceur.SharedKernel.Mixins;
using Newtonsoft.Json;
using NSubstitute;
using System.Text.RegularExpressions;
using Lanceur.Tests.Tooling;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.BusinessLogic
{
    public class SettingsMementoManagerShould
    {
        #region Fields

        /// <remarks>
        ///  Spacing is very important. If you change this value,
        ///  Check the regular expression to be sure it'll work
        ///  as expected.
        /// </remarks>
        private const string _jsonAppConfig = @"
{
    ""HotKey"": {
        ""Key"": 18,
        ""ModifierKey"": 3
    },
    ""Repository"": {
        ""ScoreLimit"": 0
    },
    ""RestartDelay"": 500.0,
    ""Window"": {
        ""Position"": {
            ""Left"": 2200.0,
            ""Top"": 200.0
        },
        ""ShowAtStartup"": true,
        ""ShowResult"": true
    }
}";

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        public SettingsMementoManagerShould(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        [Theory]
        // Should show state changed
        [InlineData("Key", "100", "", true)]
        [InlineData("ModifierKey", "100", "", true)]
        [InlineData("", "", "newDbPath", true)]
        // Should NOT show state changed
        [InlineData("ScoreLimit", "100", "", false)]
        [InlineData("RestartDelay", "100", "", false)]
        [InlineData("Left", "100.0", "", false)]
        [InlineData("Top", "100.0", "", false)]
        [InlineData("ShowAtStartup", "false", "", false)]
        [InlineData("ShowResult", "false", "", false)]
        public void ReturnExpectedStateChange(string property, string newValue, string secondDbPath, bool isStateChanged)
        {
            // ARRANGE
            var pattern = @$"(""{property}"": [a-zA-Z0-9.]*)";
            var regex = new Regex(pattern, RegexOptions.Singleline);
            var dbPath = Guid.NewGuid().ToString();

            // Initial state
            var initialAppConfig = JsonConvert.DeserializeObject<AppConfig>(_jsonAppConfig);
            _output.WriteJson("Initial AppConfig", initialAppConfig);

            // Second state
            var replacement = @$"""{property}"": {newValue}";
            var newJson = regex.Replace(_jsonAppConfig, replacement);
            var secondAppConfig = JsonConvert.DeserializeObject<AppConfig>(newJson);
            _output.WriteJson("Second AppConfig", secondAppConfig);
            _output.WriteLine($"Replacement: '{replacement}', Pattern: '{pattern}'");
            
            secondDbPath = secondDbPath.IsNullOrWhiteSpace() ? dbPath : secondDbPath;

            // Setup SettingsFacade
            var databaseConfig = Substitute.For<ILocalConfig>();
            databaseConfig.DbPath.Returns(dbPath, secondDbPath);

            var databaseConfigRepository = Substitute.For<ILocalConfigRepository>();
            databaseConfigRepository.Current.Returns(databaseConfig);

            var appConfigRepository = Substitute.For<IAppConfigRepository>();
            appConfigRepository.Current.Returns(initialAppConfig, secondAppConfig);

            var settingsFacade = new SettingsFacade(databaseConfigRepository, appConfigRepository);

            // ACT
            var memento = SettingsMementoManager.GetInitialState(settingsFacade);

            // ASSERT
            memento.HasStateChanged(settingsFacade)
                   .Should().Be(isStateChanged);
        }

        [Fact]
        public void ReturnsNotChangedWhenNoChange()
        {
            // ARRANGE
            // First state
            var dbPath = Guid.NewGuid().ToString();
            AppConfig initialAppConfig = JsonConvert.DeserializeObject<AppConfig>(_jsonAppConfig);
            var initialDbPath = dbPath;

            // Second state
            AppConfig secondAppConfig = JsonConvert.DeserializeObject<AppConfig>(_jsonAppConfig);
            var secondDbPath = dbPath;

            // Setup SettingsFacade
            var databaseConfig = Substitute.For<ILocalConfig>();
            databaseConfig.DbPath.Returns(initialDbPath, secondDbPath);

            var databaseConfigRepository = Substitute.For<ILocalConfigRepository>();
            databaseConfigRepository.Current.Returns(databaseConfig);

            var appConfigRepository = Substitute.For<IAppConfigRepository>();
            appConfigRepository.Current.Returns(initialAppConfig, secondAppConfig);

            var settingsFacade = new SettingsFacade(databaseConfigRepository, appConfigRepository);

            // ACT
            var memento = SettingsMementoManager.GetInitialState(settingsFacade);

            // ASSERT
            memento.HasStateChanged(settingsFacade).Should().BeFalse();
        }

        #endregion Methods
    }
}