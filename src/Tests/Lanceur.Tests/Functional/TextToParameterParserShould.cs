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
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Parameters.Count().Should().Be(parameterCount);
        }
    }

    #endregion
}