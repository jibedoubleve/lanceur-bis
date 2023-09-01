using System.Windows.Media.Imaging;
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

    #endregion Methods
}