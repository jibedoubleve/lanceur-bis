using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Core.Stores;
using Lanceur.Infra.Services;
using Lanceur.Infra.Stores;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Stores;

public class StoreShortcutServiceTest
{
    #region Stubs

    [Store("overridable")]
    private class OverridableStore : IStoreService
    {
        #region Properties

        public bool IsOverridable => true;
        public StoreOrchestration StoreOrchestration => null!;

        #endregion

        #region Methods

        public IEnumerable<QueryResult> GetAll() => [];
        public IEnumerable<QueryResult> Search(Cmdline cmdline) => [];

        #endregion
    }

    [Store("non-overridable")]
    private class NonOverridableStore : IStoreService
    {
        #region Properties

        public bool IsOverridable => false;
        public StoreOrchestration StoreOrchestration => null!;

        #endregion

        #region Methods

        public IEnumerable<QueryResult> GetAll() => [];
        public IEnumerable<QueryResult> Search(Cmdline cmdline) => [];

        #endregion
    }

    [Store("overridable-2")]
    private class AnotherOverridableStore : IStoreService
    {
        #region Properties

        public bool IsOverridable => true;
        public StoreOrchestration StoreOrchestration => null!;

        #endregion

        #region Methods

        public IEnumerable<QueryResult> GetAll() => [];
        public IEnumerable<QueryResult> Search(Cmdline cmdline) => [];

        #endregion
    }

    #endregion

    #region Helpers

    private static string TypeName<T>() => typeof(T).FullName!;

    private static StoreSection EmptySection() => new() { StoreShortcuts = [] };

    private static StoreSection SectionWith(params StoreShortcut[] shortcuts) =>
        new() { StoreShortcuts = shortcuts };

    private static StoreShortcut ShortcutFor<T>(string aliasOverride) =>
        new() { StoreType = TypeName<T>(), AliasOverride = aliasOverride };

    #endregion

    #region Tests

    [Fact]
    public void When_config_is_empty_Then_all_overridable_stores_are_returned()
    {
        var sut = new StoreShortcutService([new OverridableStore(), new AnotherOverridableStore()]);

        var result = sut.Resolve(EmptySection()).ToList();

        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.StoreType == TypeName<OverridableStore>());
        result.ShouldContain(x => x.StoreType == TypeName<AnotherOverridableStore>());
    }

    [Fact]
    public void When_config_is_empty_Then_non_overridable_stores_are_excluded()
    {
        var sut = new StoreShortcutService([new OverridableStore(), new NonOverridableStore()]);

        var result = sut.Resolve(EmptySection()).ToList();

        result.ShouldNotContain(x => x.StoreType == TypeName<NonOverridableStore>());
    }

    [Fact]
    public void When_config_is_complete_Then_config_shortcuts_are_returned_as_is()
    {
        var overridable = ShortcutFor<OverridableStore>("my-override");
        var another = ShortcutFor<AnotherOverridableStore>("my-other-override");
        var sut = new StoreShortcutService([new OverridableStore(), new AnotherOverridableStore()]);

        var result = sut.Resolve(SectionWith(overridable, another)).ToList();

        result.Count.ShouldBe(2);
    }

    [Fact]
    public void When_config_is_partial_Then_missing_overridable_stores_are_appended()
    {
        var existing = ShortcutFor<OverridableStore>("my-override");
        var sut = new StoreShortcutService([new OverridableStore(), new AnotherOverridableStore()]);

        var result = sut.Resolve(SectionWith(existing)).ToList();

        result.Count.ShouldBe(2);
        result.ShouldContain(x => x.StoreType == TypeName<AnotherOverridableStore>());
    }

    [Fact]
    public void When_store_is_in_config_and_discovered_Then_no_duplicate_is_returned()
    {
        var existing = ShortcutFor<OverridableStore>("my-override");
        var sut = new StoreShortcutService([new OverridableStore()]);

        var result = sut.Resolve(SectionWith(existing)).ToList();

        result.Count(x => x.StoreType == TypeName<OverridableStore>()).ShouldBe(1);
    }

    [Fact]
    public void When_store_has_config_override_Then_config_value_takes_precedence()
    {
        var existing = ShortcutFor<OverridableStore>("my-custom-override");
        var sut = new StoreShortcutService([new OverridableStore()]);

        var result = sut.Resolve(SectionWith(existing)).ToList();

        result.Single(x => x.StoreType == TypeName<OverridableStore>())
              .AliasOverride
              .ShouldBe("my-custom-override");
    }

    #endregion
}