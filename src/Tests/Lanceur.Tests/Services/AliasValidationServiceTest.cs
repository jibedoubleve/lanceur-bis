using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Services;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class AliasValidationServiceTest
{
    #region Methods

    private static AliasValidationService BuildService(params string?[] shortcuts)
    {
        var repository = Substitute.For<IAliasRepository>();
        repository.GetExistingAliases(Arg.Any<IEnumerable<string>>(), Arg.Any<long>()).Returns([]);
        repository.GetExistingDeletedAliases(Arg.Any<IEnumerable<string>>(), Arg.Any<long>()).Returns([]);

        var shortcutService = Substitute.For<IStoreShortcutService>();
        shortcutService.Resolve(Arg.Any<StoreSection>())
                       .Returns(shortcuts.Select(s => new StoreShortcut { AliasOverride = s }));

        return new AliasValidationService(repository, shortcutService, Substitute.For<ISection<StoreSection>>());
    }

    [Theory]
    [InlineData("&")]
    [InlineData("&, notepad")]
    [InlineData("notepad, :")]
    public void When_alias_name_is_a_store_shortcut_Then_alias_is_invalid(string names)
    {
        var service = BuildService("&", ":");
        var alias = new AliasQueryResult { Name = names, Synonyms = names, FileName = "notepad.exe" };

        service.IsValid(alias)
               .IsSuccess
               .ShouldBeFalse();
    }

    [Theory]
    [InlineData("&&")]
    [InlineData("??")]
    [InlineData("notepad")]
    [InlineData("st")] // "st" is an invalid override, the store does not use it
    public void When_alias_name_is_not_a_store_shortcut_Then_alias_is_valid(string names)
    {
        var service = BuildService("&",
            ":",
            "st",
            null);
        var alias = new AliasQueryResult { Name = names, Synonyms = names, FileName = "notepad.exe" };

        service.IsValid(alias)
               .IsSuccess
               .ShouldBeTrue();
    }

    #endregion
}