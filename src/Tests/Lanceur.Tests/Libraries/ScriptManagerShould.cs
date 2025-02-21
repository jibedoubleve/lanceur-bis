﻿using System.Reflection;
using System.SQLite.Updater;
using FluentAssertions;
using Xunit;

namespace Lanceur.Tests.Libraries;

public class ScriptManagerShould
{
    #region Fields

    private static readonly Assembly Asm = Assembly.GetExecutingAssembly();

    private const int Count = 3;
    private const string Pattern = @"Lanceur\.Tests\.Libraries\.Scripts\.script-(\d{1,3}\.{0,1}\d{1,3}\.{0,1}\d{0,3}).*.sql";

    #endregion

    #region Methods

    [Theory]
    [InlineData("0.1", 2)]
    [InlineData("0.2", 1)]
    [InlineData("0.2.1", 0)]
    public void ExecuteExpectedScriptAfterGivenVersion(string version, int expectedCount)
    {
        var ver = new Version(version);
        var manager = new ScriptManager(Asm, Pattern);
        var scripts = manager.GetScripts();

        scripts.After(ver)
               .Count()
               .Should()
               .Be(expectedCount);
    }

    [Fact]
    public void GetListOfResources()
    {
        var manager = new ScriptManager(Asm, Pattern);

        manager.ListResources().Should().HaveCount(Count);
    }

    [Fact]
    public void HaveResourceWithSpecifiedVersion()
    {
        var manager = new ScriptManager(Asm, Pattern);

        manager.GetResource("Lanceur.Tests.Libraries.Scripts.script-0.1.sql").Should().NotBeNull();
    }

    [Fact]
    public void HaveSql()
    {
        var manager = new ScriptManager(Asm, Pattern);

        foreach (var script in manager.GetScripts()) script.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ReturnDictionaryOfResources()
    {
        var manager = new ScriptManager(Asm, Pattern);
        manager.GetResources().Should().HaveCount(Count);
    }

    [Fact]
    public void ReturnLatestVersionOfScripts()
    {
        var version = new Version(0, 2, 1);
        var manager = new ScriptManager(Asm, Pattern);

        manager.GetScripts().MaxVersion().Should().Be(version);
    }

    [Fact]
    public void ReturnPackOfScripts()
    {
        var manager = new ScriptManager(Asm, Pattern);
        manager.GetScripts().Should().HaveCount(Count);
    }

    [Theory]
    [InlineData("0.1")]
    [InlineData("0.2")]
    [InlineData("0.2.1")]
    public void ReturnScriptFromVersion(string version)
    {
        var ver = new Version(version);
        var manager = new ScriptManager(Asm, Pattern);

        var scripts = manager.GetScripts();

        scripts[ver].Should().NotBeNull();
    }

    [Theory]
    [InlineData("1.0.0", 2)]
    [InlineData("1.0.1", 1)]
    [InlineData("1.1.1", 0)]
    public void UpdateAfter(string version, int count)
    {
        var dico = new Dictionary<Version, string> { { new("1.0.0"), "" }, { new("1.0.1"), "" }, { new("1.1.1"), "" } };
        var scripts = new ScriptCollection(dico);

        scripts.After(new(version)).Should().HaveCount(count);
    }

    #endregion
}