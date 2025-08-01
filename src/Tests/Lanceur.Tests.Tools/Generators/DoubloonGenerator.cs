using System.Collections;
using Lanceur.Tests.Tools.SQL;

namespace Lanceur.Tests.Tools.Generators;

public class DoubloonGenerator : IEnumerable<object[]>
{
    #region Fields

    private const string Arguments = "Some Parameters";
    private const string FileName = "Some/File/Name";

    #endregion

    #region Methods

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static ISqlGenerator WithDifferentSynonyms()
    {
        return new SqlGenerator()
               .AppendAlias(
                   1,
                   a => a.WithFileName(FileName)
                         .WithSynonyms("a1", "a2", "a3")
                         .WithAdditionalParameters(
                             ("params1", "params one"),
                             ("params2", "params two")
                         )
               )
               .AppendAlias(
                   2,
                   a => a.WithFileName(FileName)
                         .WithSynonyms("a4", "a5", "a6")
                         .WithAdditionalParameters(
                             ("params3", "params three"),
                             ("params4", "params four")
                         )
               );
    }

    private static ISqlGenerator WithSynonymsDoubloons()
    {
        return new SqlGenerator()
               .AppendAlias(
                   1,
                   a => a.WithFileName(FileName)
                         .WithArguments(Arguments)
                         .WithSynonyms("a1", "a2", "a3")
                         .WithAdditionalParameters(
                             ("params1", "params one"),
                             ("params2", "params two")
                         )
               )
               .AppendAlias(
                   2,
                   a => a.WithFileName(FileName)
                         .WithArguments(Arguments)
                         .WithSynonyms("a4", "a5", "a6")
                         .WithAdditionalParameters(("params1", "params one"))
               )
               .AppendAlias(
                   3,
                   a => a.WithFileName(FileName)
                         .WithArguments(Arguments)
                         .WithSynonyms("a4", "a5", "a6")
                         .WithAdditionalParameters(("params1", "params one"))
               );
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [WithSynonymsDoubloons()];
        yield return [WithDifferentSynonyms()];
    }

    #endregion
}