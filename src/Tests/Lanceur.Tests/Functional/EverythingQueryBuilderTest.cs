using Lanceur.Infra.Stores.Everything;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public sealed class EverythingQueryBuilderTest
{
    #region Methods

    [Fact]
    public void When_combining_filters_Then_query_contains_all_switches()
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
    public void When_excluding_hidden_files_repeatedly_Then_switch_appears_once()
    {
        var query = new EverythingQueryBuilder().ExcludeHiddenFiles()
                                                .ExcludeHiddenFiles()
                                                .ExcludeHiddenFiles()
                                                .BuildQuery();
        query.ShouldBe($"!{EverythingModifiers.IncludeHiddenFilesSwitch}");
    }

    [Fact]
    public void When_excluding_system_files_repeatedly_Then_switch_appears_once()
    {
        var query = new EverythingQueryBuilder().ExcludeSystemFiles()
                                                .ExcludeSystemFiles()
                                                .ExcludeSystemFiles()
                                                .BuildQuery();
        query.ShouldBe($"!{EverythingModifiers.IncludeSystemFilesSwitch}");
    }

    [Fact]
    public void When_query_has_exec_switch_Then_only_executables_are_selected()
    {
        const string query = EverythingModifiers.OnlyExecFilesSwitch;
        var adapter = new EverythingQueryAdapter(query);
        adapter.SelectOnlyExecutable.ShouldBeTrue();
    }

    [Fact]
    public void When_query_has_hidden_switch_Then_hidden_files_are_not_excluded()
    {
        const string query = $"{EverythingModifiers.IncludeHiddenFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsHiddenFilesExcluded.ShouldBeFalse();
    }

    [Fact]
    public void When_query_has_system_switch_Then_system_files_are_not_excluded()
    {
        const string query = $"{EverythingModifiers.IncludeSystemFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsSystemFilesExcluded.ShouldBeFalse();
    }

    [Fact]
    public void When_query_has_trash_bin_exclusion_Then_trash_bin_files_are_excluded()
    {
        const string query = EverythingModifiers.ExcludeFileInTrashBin;
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsFilesInTrashBinExcluded.ShouldBeTrue();
    }

    [Fact]
    public void When_query_negates_exec_switch_Then_executables_are_excluded()
    {
        const string query = $"!{EverythingModifiers.OnlyExecFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.SelectOnlyExecutable.ShouldBeTrue();
    }

    [Fact]
    public void When_query_negates_hidden_switch_Then_hidden_files_are_excluded()
    {
        const string query = $"!{EverythingModifiers.IncludeHiddenFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsHiddenFilesExcluded.ShouldBeTrue();
    }

    [Fact]
    public void When_query_negates_system_switch_Then_system_files_are_excluded()
    {
        const string query = $"!{EverythingModifiers.IncludeSystemFilesSwitch}";
        var adapter = new EverythingQueryAdapter(query);
        adapter.IsSystemFilesExcluded.ShouldBeTrue();
    }

    [Fact]
    public void When_selecting_exec_files_repeatedly_Then_switch_appears_once()
    {
        var query = new EverythingQueryBuilder().OnlyExecFiles()
                                                .OnlyExecFiles()
                                                .OnlyExecFiles()
                                                .BuildQuery();
        query.ShouldBe(EverythingModifiers.OnlyExecFilesSwitch);
    }

    #endregion
}