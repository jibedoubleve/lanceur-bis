using System.Collections;
using Lanceur.Tests.Tools.SQL;

namespace Lanceur.Tests.ViewModels.Generators;

public class DoubloonGenerator : IEnumerable<object[]>
{
    #region Fields

    private const string Arguments = "Some Parameters";
    private const string FileName = "Some/File/Name";

    #endregion

    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static SqlBuilder WithDifferentSynonyms()
    {
        return new SqlBuilder().AppendAlias(
                                   1,
                                   FileName,
                                   Arguments,
                                   cfg: alias =>
                                   {
                                       alias.WithSynonyms("a1", "a2", "a3")
                                            .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                   }
                               )
                               .AppendAlias(
                                   2,
                                   FileName,
                                   Arguments,
                                   cfg: alias =>
                                   {
                                       alias.WithSynonyms("a4", "a5", "a6")
                                            .WithArguments(new() { ["params3"] = "params three", ["params4"] = "params four" });
                                   }
                               );
    }

    private static SqlBuilder WithSynonymsDoubloons()
    {
        return new SqlBuilder().AppendAlias(
                                   1,
                                   FileName,
                                   Arguments,
                                   cfg: alias =>
                                   {
                                       alias.WithSynonyms("a1", "a2", "a3")
                                            .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                   }
                               )
                               .AppendAlias(
                                   2,
                                   FileName,
                                   Arguments,
                                   cfg: alias =>
                                   {
                                       alias.WithSynonyms("a4", "a5", "a6")
                                            .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                   }
                               )
                               .AppendAlias(
                                   3,
                                   FileName,
                                   Arguments,
                                   cfg: alias =>
                                   {
                                       alias.WithSynonyms("a4", "a5", "a6")
                                            .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                   }
                               );
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [WithSynonymsDoubloons()];
        yield return [WithDifferentSynonyms()];
    }

    #endregion
}