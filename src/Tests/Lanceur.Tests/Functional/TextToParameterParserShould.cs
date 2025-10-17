using Shouldly;
using Lanceur.Core.BusinessLogic;
using Xunit;

namespace Lanceur.Tests.Functional;

public class TextToParameterParserShould
{
    #region Methods

    public static IEnumerable<object[]> FeedParseAdditionalParameters()
    {
        yield return
        [
            """
            para1, undeuxtrois
            para2, quatrecinq

            """,
            2
        ];
        yield return
        [
            """
            para1,undeuxtrois
            para2,quatrecinq
            """,
            2
        ];
        yield return
        [
            """
            para1 , undeuxtrois
            para2 , quatrecinq
            """,
            2
        ];
    }

    [Theory]
    [MemberData(nameof(FeedParseAdditionalParameters))]
    public void ParseAdditionalParameters(string additionalParameters, int parameterCount)
    {
        var parser = new TextToParameterParser();
        var result = parser.Parse(additionalParameters);
        result.ShouldSatisfyAllConditions(
            () => result.ShouldNotBeNull(),
            () => result.Success.ShouldBeTrue(),
            () => result.Parameters.Count().ShouldBe(parameterCount)
        );
    }

    #endregion
}