using Lanceur.Infra.Stores.Everything;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public class EverythingQueryBuilderShould
{
    #region Methods

    [Fact]
    public void ExcludeFilesInTrashBin()
    {
        const string query = EverythingModifiers.ExcludeFileInTrashBin;
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsFilesInTrashBinExcluded.ShouldBeTrue();
    }

    [Fact]
    public void ExcludeHiddenFiles()
    {
        const string query = $"!{EverythingModifiers.IncludeHiddenFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsHiddenFilesExcluded.ShouldBeTrue();
    }

    [Fact]
    public void ExcludeSystemFiles()
    {
        const string query = $"!{EverythingModifiers.IncludeSystemFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsSystemFilesExcluded.ShouldBeTrue();
    }

    [Fact]
    public void NotExcludeExecFiles()
    {
        const string query = $"!{EverythingModifiers.OnlyExecFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.SelectOnlyExecutable.ShouldBeTrue();
    }

    [Fact]
    public void NotExcludeHiddenFiles()
    {
        const string query = $"{EverythingModifiers.IncludeHiddenFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsHiddenFilesExcluded.ShouldBeFalse();
    }

    [Fact]
    public void NotExcludeSystemFiles()
    {
        const string query = $"{EverythingModifiers.IncludeSystemFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsSystemFilesExcluded.ShouldBeFalse();
    }

    [Fact]
    public void SupportComplexQuery()
    {
        var query = new EverythingQueryBuilder().ExcludeSystemFiles()
                                                .ExcludeHiddenFiles()
                                                .OnlyExecFiles()
                                                .BuildQuery();
        query.ShouldBe(
            $"!{EverythingModifiers.IncludeSystemFilesSwitch} !{EverythingModifiers.IncludeHiddenFilesSwitch} {EverythingModifiers.OnlyExecFilesSwitch}"
        );
    }

    [Fact]
    public void SupportMultipleInclude_HiddenFiles()
    {
        var query = new EverythingQueryBuilder().ExcludeHiddenFiles()
                                                .ExcludeHiddenFiles()
                                                .ExcludeHiddenFiles()
                                                .BuildQuery();
        query.ShouldBe($"!{EverythingModifiers.IncludeHiddenFilesSwitch}");
    }

    [Fact]
    public void SupportMultipleInclude_OnlyExecFile()
    {
        var query = new EverythingQueryBuilder().OnlyExecFiles()
                                                .OnlyExecFiles()
                                                .OnlyExecFiles()
                                                .BuildQuery();
        query.ShouldBe(EverythingModifiers.OnlyExecFilesSwitch);
    }

    [Fact]
    public void SupportMultipleInclude_SystemFiles()
    {
        var query = new EverythingQueryBuilder().ExcludeSystemFiles()
                                                .ExcludeSystemFiles()
                                                .ExcludeSystemFiles()
                                                .BuildQuery();
        query.ShouldBe($"!{EverythingModifiers.IncludeSystemFilesSwitch}");
    }

    #endregion
}