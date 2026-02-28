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
            new SqlBuilder().AppendAlias(a => a.WithFileName("null")
                                               .WithArguments(arguments)
                                               .WithSynonyms("a1", "a2", "a3")
                                               .WithAdditionalParameters(
                                                   ("params1", "params one"),
                                                   ("params2", "params two")
                                               )
                            )
                            .AppendAlias(a => a.WithFileName("null")
                                               .WithArguments(arguments)
                                               .WithSynonyms("a4", "a5", "a6")
                                               .WithAdditionalParameters(
                                                   ("params4", "params four"),
                                                   ("params5", "params five")
                                               )
                            )
        ];
        yield return
        [
            "Two doubloons with null arguments.",
            2,
            new SqlBuilder().AppendAlias(a => a.WithFileName(fileName)
                                               .WithArguments("null")
                                               .WithSynonyms("a1", "a2", "a3")
                                               .WithAdditionalParameters(
                                                   ("params1", "params one"),
                                                   ("params2", "params two")
                                               )
                            )
                            .AppendAlias(a => a.WithFileName(fileName)
                                               .WithArguments("null")
                                               .WithSynonyms("a4", "a5", "a6")
                                               .WithAdditionalParameters(
                                                   ("params4", "params four"),
                                                   ("params5", "params five")
                                               )
                            )
        ];
        yield return
        [
            "No doubloons with null arguments.",
            0,
            new SqlBuilder()
                .AppendAlias(a => a.WithRandomFileName()
                                   .WithArguments("null")
                                   .WithSynonyms("a1")
                )
                .AppendAlias(a => a.WithRandomFileName()
                                   .WithArguments("null")
                                   .WithSynonyms("a2")
                )
        ];
        yield return
        [
            "Two doubloons with null arguments and a non doubloon.",
            2,
            new SqlBuilder()
                .AppendAlias(a => a.WithRandomFileName()
                                   .WithArguments("null")
                                   .WithSynonyms("a1")
                )
                .AppendAlias(a => a.WithFileName(fileName)
                                   .WithArguments("null")
                                   .WithSynonyms("a2")
                )
                .AppendAlias(a => a.WithFileName(fileName)
                                   .WithArguments("null")
                                   .WithSynonyms("a2")
                )
        ];
    }

    #endregion
}