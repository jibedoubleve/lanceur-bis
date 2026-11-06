using Lanceur.Core.BusinessLogic;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Functional;

public sealed class TextToParameterParserTest
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
    public void When_parsing_additional_parameters_Then_all_lines_are_parsed(string additionalParameters, int parameterCount)
    {
        var parser = new TextToParameterParser();
        var result = parser.Parse(additionalParameters);
        result.ShouldSatisfyAllConditions(
            () => result.ShouldNotBeNull(),
            () => result.Success.ShouldBeTrue(),
            () => result.Parameters.Count().ShouldBe(parameterCount)
        );
    }

    /// <summary>
    ///     Regression test for issue #1420. Quotes surrounding the parameter are removed when parsing.
    /// </summary>
    [Theory]
    [InlineData("para1,\"undeuxtrois\"")]
    [InlineData("para1, \"undeuxtrois\" ")]
    public void When_parsing_quoted_additional_parameters_Then_quotes_are_removed(string additionalParameters)
    {
        // ARRANGE
        var parser = new TextToParameterParser();

        // ACT
        var result = parser.Parse(additionalParameters);

        // ASSERT
        result.ShouldSatisfyAllConditions(
            () => result.Success.ShouldBeTrue(),
            () => result.Parameters.Single().Name.ShouldBe("para1"),
            () => result.Parameters.Single().Parameter.ShouldBe("undeuxtrois")
        );
    }

    #endregion
}