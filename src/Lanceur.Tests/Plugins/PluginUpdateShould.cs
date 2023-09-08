using FluentAssertions;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Plugins;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Plugins;

public class PluginUpdateShould
{
    #region Methods

    [Theory]
    [InlineData("1.0.0", "2.0.0")]
    [InlineData("1.0.0", "1.0.1")]
    [InlineData("1.0.0", "1.1.0")]
    public async Task SelectExistingPluginIfVersionIsHigher(string localVersion, string webVersion)
    {
        // ARRANGE
        var localManifestRepository = Substitute.For<IPluginManifestRepository>();
        localManifestRepository.GetPluginManifests().Returns(new IPluginManifest[]
        {
            new PluginManifest() { Dll = "A", Version = new(localVersion) }
        });
        
        var webManifestRepository = Substitute.For<IPluginWebManifestLoader>();
        webManifestRepository.LoadFromWebAsync().Returns(new IPluginWebManifest[]
        {
            new PluginWebManifest() { Dll = "A", Version = new Version(webVersion) }
        });

        var repository = new PluginWebRepository(localManifestRepository, webManifestRepository);

        // ACT
        var installablePlugins = await repository.GetPluginListAsync();

        // ASSERT
        installablePlugins.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0")]
    [InlineData("1.0.0", "0.0.9")]
    [InlineData("1.0.0", "0.9.0")]
    public async Task SelectNoPluginIfVersionIsLowerOrEqual(string localVersion, string webVersion)
    {
        // ARRANGE
        var localManifests = new IPluginManifest[]
        {
            new PluginManifest() { Dll = "A", Version = new(localVersion) }
        };
        var localManifestRepository = Substitute.For<IPluginManifestRepository>();
        localManifestRepository.GetPluginManifests().Returns(localManifests);

        var webManifests = new IPluginWebManifest[]
        {
            new PluginWebManifest() { Dll = "A", Version = new Version(webVersion) }
        };
        var webManifestRepository = Substitute.For<IPluginWebManifestLoader>();
        webManifestRepository.LoadFromWebAsync().Returns(webManifests);

        var repository = new PluginWebRepository(localManifestRepository, webManifestRepository);

        // ACT
        var installablePlugins = await repository.GetPluginListAsync();

        // ASSERT
        installablePlugins.Should().BeEmpty();
    }

    [Theory]
    [InlineData("1.0.0", "1.0.1", true)]
    [InlineData("1.0.0", "1.1.0", true)]
    [InlineData("1.0.0", "2.0.0", true)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("1.0.0", "0.0.1", false)]
    [InlineData("1.0.0", "0.1.0", false)]
    public void ValidateOnlyWhenAlreadyInstalledPluginHasLowerVersion(string versionInstalled, string versionToInstall,
        bool expectedValue)
    {
        // ARRANGE
        var path = Guid.NewGuid().ToString();
        var manifestInstalled = new IPluginManifest[]
        {
            new PluginManifest()
            {
                Dll = path,
                Version = new(versionInstalled)
            }
        };
        var manifestRepository = Substitute.For<IPluginManifestRepository>();
        manifestRepository.GetPluginManifests().Returns(manifestInstalled);

        var validationRule = new PluginValidationRule(manifestRepository);
        var manifestToInstall = new PluginManifest()
        {
            Dll = path,
            Version = new(versionToInstall)
        };

        // ACT
        var result = validationRule.Check(manifestToInstall);

        // ASSERT
        result
            .IsValid
            .Should().Be(expectedValue);
    }

    [Fact]
    public void ValidationSucceedsWhenNoPreinstalledPlugin()
    {
        // ARRANGE
        var path = Guid.NewGuid().ToString();
        var manifestRepository = Substitute.For<IPluginManifestRepository>();
        manifestRepository.GetPluginManifests().Returns(Array.Empty<IPluginManifest>());

        var validationRule = new PluginValidationRule(manifestRepository);
        var manifestToInstall = new PluginManifest()
        {
            Dll = path,
            Version = new("1.0.0")
        };

        // ACT
        var result = validationRule.Check(manifestToInstall);

        // ASSERT
        result
            .IsValid
            .Should().BeTrue();
    }

    #endregion Methods
}