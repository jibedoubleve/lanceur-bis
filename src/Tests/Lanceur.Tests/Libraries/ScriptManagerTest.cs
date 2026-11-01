using System.Reflection;
using System.SQLite.Updater;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Libraries;

public sealed class ScriptManagerTest
{
    #region Fields

    private static readonly Assembly Asm = Assembly.GetExecutingAssembly();

    private const int Count = 3;

    private const string Pattern
        = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

    #endregion

    #region Methods

    [Theory]
    [InlineData("0.1", 2)]
    [InlineData("0.2", 1)]
    [InlineData("0.2.1", 0)]
    public void When_filtering_scripts_after_version_Then_only_newer_scripts_are_returned(string version, int expectedCount)
    {
        var ver = new Version(version);
        var manager = new ScriptManager(Asm, Pattern);
        var scripts = manager.GetScripts();

        scripts.After(ver)
               .Count()
               .ShouldBe(expectedCount);
    }

    [Fact]
    public void When_listing_resources_Then_all_resources_are_returned()
    {
        var manager = new ScriptManager(Asm, Pattern);

        manager.ListResources()
               .Count()
               .ShouldBe(Count);
    }

    [Fact]
    public void When_getting_resource_by_name_Then_it_is_found()
    {
        var manager = new ScriptManager(Asm, Pattern);

        manager.GetResource("Lanceur.Tests.Libraries.Scripts.script-0.1.sql").ShouldNotBeNull();
    }

    [Fact]
    public void When_getting_scripts_Then_none_is_empty()
    {
        var manager = new ScriptManager(Asm, Pattern);

        foreach (var script in manager.GetScripts())
        {
            script.ShouldNotBeNullOrEmpty();
        }
    }

    [Fact]
    public void When_getting_resources_Then_all_resources_are_returned()
    {
        var manager = new ScriptManager(Asm, Pattern);
        manager.GetResources().Count.ShouldBe(Count);
    }

    [Fact]
    public void When_getting_max_version_Then_latest_script_version_is_returned()
    {
        var version = new Version(0, 2, 1);
        var manager = new ScriptManager(Asm, Pattern);

        manager.GetScripts().MaxVersion().ShouldBe(version);
    }

    [Fact]
    public void When_getting_scripts_Then_all_scripts_are_returned()
    {
        var manager = new ScriptManager(Asm, Pattern);
        manager.GetScripts().Count().ShouldBe(Count);
    }

    [Theory]
    [InlineData("0.1")]
    [InlineData("0.2")]
    [InlineData("0.2.1")]
    public void When_indexing_scripts_by_version_Then_script_is_returned(string version)
    {
        var ver = new Version(version);
        var manager = new ScriptManager(Asm, Pattern);

        var scripts = manager.GetScripts();

        scripts[ver].ShouldNotBeNull();
    }

    [Theory]
    [InlineData("1.0.0", 2)]
    [InlineData("1.0.1", 1)]
    [InlineData("1.1.1", 0)]
    public void When_filtering_collection_after_version_Then_only_newer_versions_are_returned(string version, int count)
    {
        var dico = new Dictionary<Version, string>
            { { new Version("1.0.0"), "" }, { new Version("1.0.1"), "" }, { new Version("1.1.1"), "" } };
        var scripts = new ScriptCollection(dico);

        scripts.After(new Version(version)).Count().ShouldBe(count);
    }

    #endregion
}