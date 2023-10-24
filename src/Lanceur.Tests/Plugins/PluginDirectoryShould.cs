using FluentAssertions;
using Lanceur.Core.Plugins;
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

        [Theory]
        [InlineData(true, "directory/plugin.dll")]
        [InlineData(true, "/directory/plugin.dll")]
        [InlineData(false, "directory")]
        [InlineData(false, "directory/")]
        [InlineData(false, "/directory")]
        public void BeInMyDocuments(bool isFile, string dir)
        {
            // Arrange
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var expectedDirectory = new DirectoryInfo(Path.Combine(myDocuments, Locations.RelativePath, dir.Trim('\\', '/'))).FullName;

            if (isFile)
            {
                expectedDirectory = Path.GetDirectoryName(expectedDirectory);
            }

            _output.WriteLine($"My Documents: {myDocuments}");
            _output.WriteLine($"Plugin dir  : {dir}");
            _output.WriteLine($"Full path   : {expectedDirectory}");

            // Act
            var path = isFile
                ? Locations.FromFile(dir)
                : Locations.FromDirectory(dir);

            // Assert
            path.PluginDirectory.FullName.Should().Be(expectedDirectory);
        }

        [Fact]
        public void BeInMyDocumentsWithPluginManifest()
        {
            // Arrange
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            const string dir = "directory;";

            var expectedDirectory = new DirectoryInfo(Path.Combine(myDocuments, Locations.RelativePath, dir));

            _output.WriteLine($"My Documents: {myDocuments}");
            _output.WriteLine($"Plugin dir  : {dir}");
            _output.WriteLine($"Full path   : {expectedDirectory}");

            var manifest = Substitute.For<IPluginManifest>();
            manifest.Dll.Returns(@$"{dir}/plugin.dll");

            // Act
            var path = Locations.FromManifest(manifest);

            // Assert
            path.PluginDirectory.FullName.Should().Be(expectedDirectory.FullName);
        }

        #endregion Methods
    }
}