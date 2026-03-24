using System.Collections.ObjectModel;
using Bogus;
using Lanceur.Core.Models;
using Newtonsoft.Json;
using ScottPlot;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Tools.StateTesters;

public static class AliasBuilder
{
    #region Methods

    private static AdditionalParameter[] GetAdditionalParameters(Faker faker)
    {
        AdditionalParameter[] additionalParameters =
        [
            new() { Name = faker.Lorem.Word() + "_" + Generate.RandomString(8), Parameter = "1" },
            new() { Name = faker.Lorem.Word() + "_" + Generate.RandomString(8), Parameter = "2" },
            new() { Name = faker.Lorem.Word() + "_" + Generate.RandomString(8), Parameter = "3" },
            new() { Name = faker.Lorem.Word() + "_" + Generate.RandomString(8), Parameter = "4" }
        ];
        return additionalParameters;
    }

    private static string[] GetStrings(Faker faker)
    {
        string[] names =
        [
            Generate.RandomString(8) + "_" + faker.Lorem.Word(),
            Generate.RandomString(8) + "_" + faker.Lorem.Word(),
            Generate.RandomString(8) + "_" + faker.Lorem.Word()
        ];
        return names;
    }

    private static readonly Faker Faker = new();
    public static AliasQueryResult BuildRandomAlias()
    {
        var names = GetStrings(Faker);

        return new AliasQueryResult
        {
            FileName = Path.Combine(@"C:\Random\Path", Path.GetTempFileName()),
            WorkingDirectory = Path.GetTempPath(),
            LuaScript = Faker.Lorem.Sentence(),
            Name = names[0],
            Synonyms = string.Join(",", names),
            Description = Faker.Lorem.Sentence(),
            Parameters = string.Join(", ", GetStrings(Faker)),
            AdditionalParameters = new ObservableCollection<AdditionalParameter>(
                GetAdditionalParameters(Faker)
            )
        };
    }

    public static void CopyState(this AliasQueryResult destination, AliasQueryResult source)
    {
        destination.FileName = source.FileName;
        destination.WorkingDirectory = source.WorkingDirectory;
        destination.LuaScript = source.LuaScript;
        destination.Name = source.Name;
        destination.Synonyms = source.Synonyms;
        destination.Description = source.DescriptionDisplay;
        destination.Parameters = source.Parameters;
        destination.AdditionalParameters = source.AdditionalParameters;
    }

    public static void SetRandomState(this AliasQueryResult source)
    {
        var names = GetStrings(Faker);

        source.FileName = Path.Combine(@"C:\Random\Path", Path.GetTempFileName());
        source.WorkingDirectory = Path.GetTempPath();
        source.LuaScript = Faker.Lorem.Sentence();
        source.Name = names[0];
        source.Synonyms = string.Join(",", names);
        source.Description = Faker.Lorem.Sentence();
        source.Parameters = string.Join(", ", GetStrings(Faker));
        source.AdditionalParameters = new ObservableCollection<AdditionalParameter>(
            GetAdditionalParameters(Faker)
        );
    }

    public static void ShouldHaveState(this QueryResult source, QueryResult expectedState)
    {
        expectedState.ShouldBeOfType<AliasQueryResult>();
        var expected = (AliasQueryResult)expectedState;

        source.ShouldBeOfType<AliasQueryResult>();
        var alias = (AliasQueryResult)source;

        alias.ShouldSatisfyAllConditions(
            a => a.ShouldNotBeNull(),
            a => a.FileName.ShouldBe(expected.FileName),
            a => a.LuaScript.ShouldBe(expected.LuaScript),
            a => a.Name.ShouldBe(expected.Name),
            a => a.DescriptionDisplay.ShouldBe(expected.Description),
            a => a.Parameters.ShouldBe(expected.Parameters),
            a => a.WorkingDirectory.ShouldBe(expected.WorkingDirectory),
            a => a.Description.ShouldBe(expected.Description)
        );
    }

    public static void WriteAlias(this ITestOutputHelper output, QueryResult alias)
    {
        var json = JsonConvert.SerializeObject(alias, Formatting.Indented);
        output.WriteLine("Output of alias:");
        output.WriteLine("================");
        output.WriteLine(json);
    }

    #endregion
}