using Bogus;
using Shouldly;
using Lanceur.Core.Models;
using Lanceur.SharedKernel;
using Xunit;

namespace Lanceur.Tests.Tools.StateTesters;

public class AliasStateTester
{
    #region Fields

    public static readonly List<AdditionalParameter> TestAdditionalParameter = [new() { Name = "one", Parameter = "1" }, new() { Name = "two", Parameter = "2" }, new() { Name = "three", Parameter = "3" }, new() { Name = "four", Parameter = "4" }];

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

    private string FileName { get;  }
    private string LuaScript { get;  }
    public string Name { get;  }
    private string Name2 { get;  }
    private string Name3 { get;  }
    private string Description { get;  }
    private string Parameters { get;  }
    private string WorkingDirectory { get;  }

    #endregion

    #region Methods

    public void AssertValues(AliasQueryResult alias)
    {
        if (alias is null) Assert.Fail($"{nameof(alias)} should not be null for assertion.");

        alias.FileName.Should().Be(FileName);
        alias.LuaScript.Should().Be(LuaScript);
        alias.Name.Should().Be(Name);
        alias.DescriptionDisplay.Should().Be(Description);
        alias.Parameters.Should().Be(Parameters);
        alias.WorkingDirectory.Should().Be(WorkingDirectory);
        alias.Description.Should().Be(Description);
    }

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
        alias.AdditionalParameters = new(TestAdditionalParameter);
    }

    #endregion
}