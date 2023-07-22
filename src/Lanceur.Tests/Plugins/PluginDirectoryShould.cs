using FluentAssertions;
using Lanceur.Core.Models;
using Lanceur.Infra.Plugins;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lanceur.Tests.Plugins
{
    public class PluginDirectoryShould
    {
        #region Fields

        private readonly ITestOutputHelper _output;

        #endregion Fields

        #region Constructors

        public PluginDirectoryShould(ITestOutputHelper output)
        {
            _output = output;
        }

        #endregion Constructors

        #region Methods

        [Fact]
        public void BeInMyDocuments()
        {
            // Arrange
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = "directory;";

            var expectedDirectory = new DirectoryInfo(Path.Combine(myDocuments, PluginDirectory.RelativePath, dir));

            _output.WriteLine($"My Documents: {myDocuments}");
            _output.WriteLine($"Plugin dir  : {dir}");
            _output.WriteLine($"Full path   : {expectedDirectory}");

            // Act
            var path = new PluginDirectory(dir);

            // Assert
            path.Directory.FullName.Should().Be(expectedDirectory.FullName);
        }

        [Fact]
        public void BeInMyDocumentsWithPluginManifest()
        {
            // Arrange
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = "directory;";

            var expectedDirectory = new DirectoryInfo(Path.Combine(myDocuments, PluginDirectory.RelativePath, dir));

            _output.WriteLine($"My Documents: {myDocuments}");
            _output.WriteLine($"Plugin dir  : {dir}");
            _output.WriteLine($"Full path   : {expectedDirectory}");

            var manifest = Substitute.For<IPluginConfiguration>();
            manifest.Dll.Returns(@$"{dir}/plugin.dll");

            // Act
            var path = new PluginDirectory(manifest);

            // Assert
            path.Directory.FullName.Should().Be(expectedDirectory.FullName);
        }

        #endregion Methods
    }
}