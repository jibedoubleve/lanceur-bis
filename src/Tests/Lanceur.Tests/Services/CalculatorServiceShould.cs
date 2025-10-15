using System.Reflection;
using System.Text.RegularExpressions;
using Shouldly;
using Lanceur.Infra.Services;
using Lanceur.SharedKernel.Logging;
using Lanceur.Tests.Tooling.Logging;
using Lanceur.Tests.Tools.Logging;
using Xunit;

namespace Lanceur.Tests.Services;

public class CalculatorServiceShould
{
    #region Fields

    private readonly ITestOutputHelper _output;

    private readonly MicrosoftLoggingLoggerFactory _testLoggerFactory;

    #endregion

    #region Constructors

    public CalculatorServiceShould(ITestOutputHelper output)
    {
        _output = output;
        _testLoggerFactory = new(output);
    }

    #endregion

    #region Methods

    public static IEnumerable<object[]> GetMathFunctions()
    {
        IEnumerable<string> operations1 =
        [
            "Abs", "Acos", "Asin", "Atan", "Ceiling", "Cos", "Exp", "Floor", "IEEERemainder", "Ln", "Log", "Log10",
            "Max", "Min", "Pow", "Round", "Sign", "Sin", "Sqrt", "Tan", "Truncate", "in", "if", "ifs"
        ];
        operations1 = operations1.ToArray();
        var operations2 = operations1.Select(e => $"       {e}").ToArray();

        return operations1.Concat(operations2)
                          .Select(e => new object[] { e });
    }

    [Theory]
    [MemberData(nameof(GetMathFunctions), MemberType = typeof(CalculatorServiceShould))]
    public void HandleMathematicalExpression(string operation)
    {
        var calculator = new NCalcCalculatorService(_testLoggerFactory.GetLogger<NCalcCalculatorService>());

        _output.WriteLine($"Regex: {calculator!.ValidationRegex}");


        var regex = new Regex(calculator!.ValidationRegex);

        _output.WriteLine($"Expression: {operation}");
        regex.IsMatch(operation).Should().BeTrue("'operation' is valid");
    }

    [Theory]
    [InlineData("6+5")]
    [InlineData("(4+4)+5")]
    [InlineData("45 + (689)")]
    [InlineData("( 10 + 10")]
    [InlineData("45  + (")]
    [InlineData(" 45  + (")]
    [InlineData("0")]
    [InlineData(" 0")]
    public void HandleNumbers_NCalc(string expression)
    {
        var calculator = new NCalcCalculatorService(_testLoggerFactory.GetLogger<NCalcCalculatorService>());
        _output.WriteLine($"Regex: {calculator!.ValidationRegex}");
        var regex = new Regex(calculator!.ValidationRegex);
        regex.IsMatch(expression).Should().BeTrue();
    }

    [Theory]
    [InlineData("lkj")]
    public void ReturnResultOnError(string expression)
    {
        var calculator = new NCalcCalculatorService(
            new TestOutputHelperDecoratorForMicrosoftLogging<NCalcCalculatorService>(_output)
        );

        using (new AssertionScope())
        {
            calculator.Evaluate(expression).IsError.Should().BeTrue();
            calculator.Evaluate(expression).Result.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("1+1", "2")]
    [InlineData("2*3", "6")]
    [InlineData("2-3", "-1")]
    [InlineData("9/3", "3")]
    [InlineData("Sqrt(9)", "3")]
    [InlineData("sqrt(9)", "3")]
    [InlineData("(8+2) * 2 ", "20")]
    public void ReturnResultOnSuccess(string expression, string expected)
    {
        var calculator = new NCalcCalculatorService(
            new TestOutputHelperDecoratorForMicrosoftLogging<NCalcCalculatorService>(_output)
        );

        using (new AssertionScope())
        {
            var result = calculator.Evaluate(expression);
            result.IsError.Should().BeFalse();
            result.Result.Should().Be(expected);
        }
    }

    #endregion
}