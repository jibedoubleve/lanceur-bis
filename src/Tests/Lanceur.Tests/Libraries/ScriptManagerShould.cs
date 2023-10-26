using FluentAssertions;
using System.Reflection;
using System.SQLite.Updater;
using Xunit;

namespace Lanceur.Tests.Libraries
{
    public class ScriptManagerShould
    {
        #region Fields

        private const int COUNT = 3;
        private const string pattern = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";
        private static readonly Assembly asm = Assembly.GetExecutingAssembly();

        #endregion Fields

        #region Methods

        [Theory]
        [InlineData("0.1", 2)]
        [InlineData("0.2", 1)]
        [InlineData("0.2.1", 0)]
        public void ExecuteExpectedScriptAfterGivenVersion(string version, int expectedCount)
        {
            var ver = new Version(version);
            var manager = new ScriptManager(asm, pattern);
            var scripts = manager.GetScripts();
            var counter = 0;

            foreach (var script in scripts.After(ver))
            {
                counter++;
            }

            counter.Should().Be(expectedCount);
        }

        [Fact]
        public void GetListOfResources()
        {
            var manager = new ScriptManager(asm, pattern);

            manager.ListResources().Should().HaveCount(COUNT);
        }

        [Fact]
        public void HaveResourceWithSpecifiedVersion()
        {
            var manager = new ScriptManager(asm, pattern);

            manager.GetResource("Lanceur.Tests.Libraries.Scripts.script-0.1.sql").Should().NotBeNull();
        }

        [Fact]
        public void HaveSQL()
        {
            var manager = new ScriptManager(asm, pattern);

            foreach (var script in manager.GetScripts())
            {
                script.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void ReturnDictionaryOfResources()
        {
            var manager = new ScriptManager(asm, pattern);
            manager.GetResources().Should().HaveCount(COUNT);
        }

        [Fact]
        public void ReturnLatestVersionOfScripts()
        {
            var version = new Version(0, 2, 1);
            var manager = new ScriptManager(asm, pattern);

            manager.GetScripts().MaxVersion().Should().Be(version);
        }

        [Fact]
        public void ReturnPackOfScripts()
        {
            var manager = new ScriptManager(asm, pattern);
            manager.GetScripts().Should().HaveCount(COUNT);
        }

        [Theory]
        [InlineData("0.1")]
        [InlineData("0.2")]
        [InlineData("0.2.1")]
        public void ReturnScriptFromVersion(string version)
        {
            var ver = new Version(version);
            var manager = new ScriptManager(asm, pattern);

            var scripts = manager.GetScripts();

            scripts[ver].Should().NotBeNull();
        }

        [Theory]
        [InlineData("1.0.0", 2)]
        [InlineData("1.0.1", 1)]
        [InlineData("1.1.1", 0)]
        public void UpdateAfter(string version, int count)
        {
            var dico = new Dictionary<Version, string>()
            {
                { new Version("1.0.0"), "" },
                { new Version("1.0.1"), "" },
                { new Version("1.1.1"), "" },
            };
            var scripts = new ScriptCollection(dico);

            scripts.After(new Version(version)).Should().HaveCount(count);
        }

        #endregion Methods
    }
}