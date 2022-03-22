using FluentAssertions;
using Lanceur.Infra;
using Lanceur.Infra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class CalculatorServiceShould
    {
        [Theory]
        [InlineData("1+1", "2")]
        [InlineData("2*3", "6")]
        [InlineData("2-3", "-1")]
        [InlineData("9/3", "3")]
        [InlineData("Sqrt(9)", "3")]
        public void ReturnExpectedResult(string expression, string expected)
        {
            var calculator = new CodingSebCalculatorService();

            calculator.Evaluate(expression).Should().Be(expected);
        }
    }
}
