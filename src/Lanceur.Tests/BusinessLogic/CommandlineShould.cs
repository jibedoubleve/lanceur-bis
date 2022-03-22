using FluentAssertions;
using Lanceur.Infra;
using Lanceur.Infra.Managers;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class CommandlineShould
    {
        [Fact]
        public void ReturnsEmptyOnEmptyCmdline()
        {
            var processor = new CmdlineProcessor();
            var line = processor.Process(string.Empty);

            line.Name.Should().BeEmpty();
            line.Parameters.Should().BeEmpty();
        }
        [Fact]
        public void ReturnsEmptyOnNullCmdline()
        {
            var processor = new CmdlineProcessor();
            var line = processor.Process(null);

            line.Name.Should().BeEmpty();
            line.Parameters.Should().BeEmpty();
        }

        [Theory]
        [InlineData("cls", "cls arg1 arg2")]
        [InlineData("excel", "excel arg1 arg2")]
        [InlineData("excel", "excel")]
        [InlineData("%", "% arg1 arg2")]
        [InlineData("$", "$arg1 arg2")]
        public void HaveName(string asExpected, string actual)
        {
            var processor = new CmdlineProcessor();
            var line = processor.Process(actual);

            line.Name.Should().Be(asExpected);
        }

        [Theory]
        [InlineData("arg1 arg2", "cls arg1 arg2")]
        [InlineData("arg1 arg2", "excel arg1 arg2")]
        [InlineData("", "excel")]
        [InlineData("arg1 arg2", "% arg1 arg2")]
        [InlineData("arg1 arg2", "$arg1 arg2")]
        public void HaveArguments(string asExpected, string actual)
        {
            var processor = new CmdlineProcessor();
            var line = processor.Process(actual);

            line.Parameters.Should().Be(asExpected);
        }
    }
}