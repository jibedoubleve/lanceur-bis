using System.Collections.ObjectModel;
using Bogus;
using Lanceur.Core.Models;
using Lanceur.SharedKernel;
using Shouldly;

namespace Lanceur.Tests.Tools.StateTesters;

public sealed class AliasStateTester
{
    #region Fields

    public static readonly List<AdditionalParameter> TestAdditionalParameter =
    [
        new() { Name = "one", Parameter = "1" },
        new() { Name = "two", Parameter = "2" },
        new() { Name = "three", Parameter = "3" },
        new() { Name = "four", Parameter = "4" }
    ];

    #endregion

    #region Constructors

    public AliasStateTester()
    {
        var faker = new Faker();
        FileName = faker.System.FileName();
        WorkingDirectory = faker.System.DirectoryPath();
        LuaScript = faker.Lorem.Sentence();
        Name = faker.Lorem.Word();
        Name2 = faker.Lorem.Word();
        Name3 = faker.Lorem.Word();
        Description = faker.Lorem.Sentence();
        Parameters = string.Join(
            ", ",
            faker.Lorem.Word(),
            faker.Lorem.Word(),
            faker.Lorem.Word()
        );
    }

    #endregion

    #region Properties

    private string Description { get; }

    private string FileName { get; }
    private string LuaScript { get; }
    private string Name2 { get; }
    private string Name3 { get; }
    private string Parameters { get; }
    private string WorkingDirectory { get; }
    public string Name { get; }

    #endregion

    #region Methods

    public void AssertValues(AliasQueryResult alias)
        => alias.ShouldSatisfyAllConditions(
            a => a.ShouldNotBeNull(),
            a => a.FileName.ShouldBe(FileName),
            a => a.LuaScript.ShouldBe(LuaScript),
            a => a.Name.ShouldBe(Name),
            a => a.DescriptionDisplay.ShouldBe(Description),
            a => a.Parameters.ShouldBe(Parameters),
            a => a.WorkingDirectory.ShouldBe(WorkingDirectory),
            a => a.Description.ShouldBe(Description)
        );

    public void UpdateValues(ref AliasQueryResult alias)
    {
        alias.Name = Name;
        alias.Synonyms = $"{Name}, {Name2}, {Name3}";
        alias.Description = Description;
        alias.FileName = FileName;
        alias.Parameters = Parameters;
        alias.WorkingDirectory = WorkingDirectory;
        alias.RunAs = Constants.RunAs.Admin;
        alias.StartMode = Constants.StartMode.Maximised;
        alias.IsExecutionConfirmationRequired = true;
        alias.LuaScript = LuaScript;
        alias.AdditionalParameters = new ObservableCollection<AdditionalParameter>(TestAdditionalParameter);
    }

    #endregion
}