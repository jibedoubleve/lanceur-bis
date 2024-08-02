using FluentAssertions;
using FluentAssertions.Execution;
using Lanceur.Infra.Services;
using Xunit;

namespace Lanceur.Tests.BusinessLogic;

public class CalculatorServiceShould
{
    #region Methods

    [Theory, InlineData("lkj")]
    public void ReturnResultOnError(string expression)
    {
        var calculator = new CodingSebCalculatorService();

        using (new AssertionScope())
        {
            calculator.Evaluate(expression).IsError.Should().BeTrue();
            calculator.Evaluate(expression).Result.Should().NotBeNull();
        }
    }

    [Theory, InlineData("1+1", "2"), InlineData("2*3", "6"), InlineData("2-3", "-1"), InlineData("9/3", "3"), InlineData("Sqrt(9)", "3"), InlineData("sqrt(9)", "3")]
    public void ReturnResultOnSuccess(string expression, string expected)
    {
        var calculator = new CodingSebCalculatorService();

        using (new AssertionScope())
        {
            calculator.Evaluate(expression).IsError.Should().BeFalse();
            calculator.Evaluate(expression).Result.Should().Be(expected);
        }
    }

    #endregion Methods
}