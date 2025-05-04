using System.Collections;
using Lanceur.Tests.Tools.SQL;

namespace Lanceur.Tests.Tools.Generators;

public class DoubloonWithNullGenerator : IEnumerable<object[]>
{
    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<object[]> GetEnumerator()
    {
        const string fileName = "FileName";
        const string arguments = "Arguments";
        yield return
        [
            "Two doubloons with null file names.",
            2,
            new SqlBuilder().AppendAlias(
                                1,
                                "null",
                                arguments,
                                cfg: alias =>
                                {
                                    alias.WithSynonyms("a1", "a2", "a3")
                                         .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                }
                            )
                            .AppendAlias(
                                2,
                                "null",
                                arguments,
                                cfg: alias =>
                                {
                                    alias.WithSynonyms("a4", "a5", "a6")
                                         .WithArguments(new() { ["params4"] = "params four", ["params5"] = "params five" });
                                }
                            )
        ];
        yield return
        [
            "Two doubloons with null arguments.",
            2,
            new SqlBuilder().AppendAlias(
                                1,
                                fileName,
                                "null",
                                cfg: alias =>
                                {
                                    alias.WithSynonyms("a1", "a2", "a3")
                                         .WithArguments(new() { ["params1"] = "params one", ["params2"] = "params two" });
                                }
                            )
                            .AppendAlias(
                                2,
                                fileName,
                                "null",
                                cfg: alias =>
                                {
                                    alias.WithSynonyms("a4", "a5", "a6")
                                         .WithArguments(new() { ["params4"] = "params four", ["params5"] = "params five" });
                                }
                            )
        ];
        yield return
        [
            "No doubloons with null arguments.",
            0,
            new SqlBuilder()
                .AppendAlias(
                    1,
                    Guid.NewGuid().ToString(),
                    "null",
                    cfg: alias => alias.WithSynonyms("a1")
                )
                .AppendAlias(
                    2,
                    Guid.NewGuid().ToString(),
                    "null",
                    cfg: alias => alias.WithSynonyms("a2")
                )
        ];
        yield return
        [
            "Two doubloons with null arguments and a non doubloon.",
            2,
            new SqlBuilder()
                .AppendAlias(
                    1,
                    Guid.NewGuid().ToString(),
                    "null",
                    cfg: alias => alias.WithSynonyms("a1")
                )
                .AppendAlias(
                    2,
                    fileName,
                    "null",
                    cfg: alias => alias.WithSynonyms("a2")
                )
                .AppendAlias(
                    3,
                    fileName,
                    "null",
                    cfg: alias => alias.WithSynonyms("a2")
                )
        ];
    }

    #endregion
}