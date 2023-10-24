using FluentAssertions;
using Lanceur.Infra.Managers;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class CommandlineShould
    {
        #region Methods

        [Theory]
        [InlineData("arg1 arg2", "cls arg1 arg2")]
        [InlineData("arg1 arg2", "excel arg1 arg2")]
        [InlineData("", "excel")]
        [InlineData("arg1 arg2", "% arg1 arg2")]
        [InlineData("arg1 arg2", "$arg1 arg2")]
        [InlineData("arg1 arg2", "? arg1 arg2")]
        [InlineData("arg1 arg2", "?arg1 arg2")]
        [InlineData("a?rg2", "arg1 a?rg2")]
        public void HaveArguments(string asExpected, string actual)
        {
            var processor = new CmdlineManager();
            var line = processor.BuildFromText(actual);

            line.Parameters.Should().Be(asExpected);
        }

        [Theory]
        [InlineData("cls", "cls arg1 arg2")]
        [InlineData("excel", "excel arg1 arg2")]
        [InlineData("excel", "excel")]
        [InlineData("%", "% arg1 arg2")]
        [InlineData("$", "$arg1 arg2")]
        [InlineData("a", "a")]
        public void HaveName(string asExpected, string actual)
        {
            var processor = new CmdlineManager();
            var line = processor.BuildFromText(actual);

            line.Name.Should().Be(asExpected);
        }

        [Theory]
        [InlineData("? hello world", "?")]
        [InlineData("??", "??")]
        [InlineData("&& un deux trois", "&&")]
        [InlineData("&", "&")]
        [InlineData("", "")]
        [InlineData("m&", "m&")]
        [InlineData("m&&", "m&&")]
        [InlineData("m& fff", "m&")]
        [InlineData("m&& fff", "m&&")]
        public void RecogniseDoubleOrSingleSpecialChar(string cmdline, string expected)
        {
            var processor = new CmdlineManager();

            processor
                .BuildFromText(cmdline)
                .Name
                .Should().Be(expected);
        }

        [Fact]
        public void ReturnsEmptyOnEmptyCmdline()
        {
            var processor = new CmdlineManager();
            var line = processor.BuildFromText(string.Empty);

            line.Name.Should().BeEmpty();
            line.Parameters.Should().BeEmpty();
        }

        [Fact]
        public void ReturnsEmptyOnNullCmdline()
        {
            var processor = new CmdlineManager();
            var line = processor.BuildFromText(null);

            line.Name.Should().BeEmpty();
            line.Parameters.Should().BeEmpty();
        }

        #endregion Methods
    }
}