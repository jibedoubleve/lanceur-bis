using FluentAssertions;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Infra.Plugins;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Plugins;

public class PluginUninstallShould
{
    #region Methods

    [Fact]
    public async Task NotDisplayCandidateToUninstall()
    {
        // ARRANGE
        var manifest = new PluginManifest() { Dll = "A", Version = new("1.0.0") };
        var localManifests = new IPluginManifest[] { manifest };
        var localManifestRepository = Substitute.For<IPluginManifestRepository>();
        localManifestRepository.GetPluginManifests().Returns(localManifests);

        var webManifests = new IPluginWebManifest[] { new PluginWebManifest() { Dll = "A", Version = new("1.0.0") } };
        var webManifestRepository = Substitute.For<IPluginWebManifestLoader>();
        webManifestRepository.LoadFromWebAsync().Returns(webManifests);

        var repository = new PluginWebRepository(localManifestRepository, webManifestRepository);

        // ACT
        var installablePlugins = await repository.GetPluginListAsync(new[] { manifest });

        // ASSERT
        installablePlugins.Should().NotBeEmpty();
    }

    #endregion Methods
}