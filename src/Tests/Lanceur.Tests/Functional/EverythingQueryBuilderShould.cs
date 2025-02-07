using FluentAssertions;
using Lanceur.Infra.Stores.Everything;
using Xunit;

namespace Lanceur.Tests.Functional;

public class EverythingQueryBuilderShould
{
    #region Methods

    [Fact]
    public void ExcludeHiddenFiles()
    {
        const string query = $"!{EverythingModifiers.IncludeHiddenFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsHiddenFilesExcluded.Should().BeTrue();
    }

    [Fact]
    public void ExcludeSystemFiles()
    {
        const string query = $"!{EverythingModifiers.IncludeSystemFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsSystemFilesExcluded.Should().BeTrue();
    }
    
    [Fact]
    public void NotExcludeExecFiles()
    {
        const string query = $"!{EverythingModifiers.OnlyExecFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.SelectOnlyExecutable.Should().BeTrue();
    }

    [Fact]
    public void NotExcludeHiddenFiles()
    {
        const string query = $"{EverythingModifiers.IncludeHiddenFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsHiddenFilesExcluded.Should().BeFalse();
    }

    [Fact]
    public void NotExcludeSystemFiles()
    {
        const string query = $"{EverythingModifiers.IncludeSystemFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsSystemFilesExcluded.Should().BeFalse();
    }

    [Fact]
    public void SupportComplexQuery()
    {
        var query = new EverythingQueryBuilder().ExcludeSystemFiles()
                                                .ExcludeHiddenFiles()
                                                .OnlyExecFiles()
                                                .ToString();
        query.Should().Be($"!{EverythingModifiers.IncludeSystemFilesSwitch} !{EverythingModifiers.IncludeHiddenFilesSwitch} {EverythingModifiers.OnlyExecFilesSwitch}");
    }

    [Fact]
    public void SupportMultipleInclude_HiddenFiles()
    {
        var query = new EverythingQueryBuilder().ExcludeHiddenFiles()
                                                .ExcludeHiddenFiles()
                                                .ExcludeHiddenFiles()
                                                .ToString();
        query.Should().Be($"!{EverythingModifiers.IncludeHiddenFilesSwitch}");
    }

    [Fact]
    public void SupportMultipleInclude_OnlyExecFile()
    {
        var query = new EverythingQueryBuilder().OnlyExecFiles()
                                                .OnlyExecFiles()
                                                .OnlyExecFiles()
                                                .ToString();
        query.Should().Be(EverythingModifiers.OnlyExecFilesSwitch);
    }

    [Fact]
    public void SupportMultipleInclude_SystemFiles()
    {
        var query = new EverythingQueryBuilder().ExcludeSystemFiles()
                                                .ExcludeSystemFiles()
                                                .ExcludeSystemFiles()
                                                .ToString();
        query.Should().Be($"!{EverythingModifiers.IncludeSystemFilesSwitch}");
    }

    #endregion
}