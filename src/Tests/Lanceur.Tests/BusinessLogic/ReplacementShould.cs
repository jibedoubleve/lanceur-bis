using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Infra.Wildcards;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.BusinessLogic
{
    public class ReplacementShould
    {
        #region Methods

        [Theory]
        [InlineData("$r$", "", "")]
        [InlineData(null, "un deux", "")]
        [InlineData("", "un deux", "")]
        [InlineData("$r$", null, "")]
        [InlineData("Hello $r$", "world", "Hello world")]
        [InlineData("Number $r$", "1", "Number 1")]
        [InlineData("Hello $r$", " ", "Hello  ")]
        [InlineData("Hello $r$", "", "Hello ")]
        [InlineData("Hello $r$", "`", "Hello `")]
        //
        [InlineData("$R$", "", "")]
        [InlineData("$R$", null, "")]
        [InlineData("Hello $R$", "world", "Hello world")]
        [InlineData("Number $R$", "1", "Number 1")]
        [InlineData("Hello $R$", " ", "Hello  ")]
        [InlineData("Hello $R$", "", "Hello ")]
        [InlineData("Hello $R$", "`", "Hello `")]
        public void ReplaceWithClipboardText(string actual, string param, string expected)
        {
            var clipboard = Substitute.For<IClipboardService>();
            clipboard.GetText().Returns(param);

            var rpl = new RawClipboardReplacement(clipboard);

            rpl.Replace(actual, null)
               .Should().Be(expected);
        }

        [Theory]
        [InlineData("$i$", "", "")]
        [InlineData(null, "un deux", "")]
        [InlineData("", "un deux", "")]
        [InlineData("$i$", null, "")]
        [InlineData("Hello $i$", "world", "Hello world")]
        [InlineData("Number $i$", "1", "Number 1")]
        [InlineData("Hello $i$", " ", "Hello  ")]
        [InlineData("Hello $i$", "", "Hello ")]
        [InlineData("Hello $i$", "`", "Hello `")]
        //
        [InlineData("$I$", "", "")]
        [InlineData("$I$", null, "")]
        [InlineData("Hello $I$", "world", "Hello world")]
        [InlineData("Number $I$", "1", "Number 1")]
        [InlineData("Hello $I$", " ", "Hello  ")]
        [InlineData("Hello $I$", "", "Hello ")]
        [InlineData("Hello $I$", "`", "Hello `")]
        public void ReplaceWithText(string actual, string param, string expected)
        {
            var rpl = new TextReplacement();

            rpl.Replace(actual, param)
               .Should().Be(expected);
        }

        [Theory]
        [InlineData("$c$", "", "")]
        [InlineData(null, "un deux", "")]
        [InlineData("", "un deux", "")]
        [InlineData("$c$", null, "")]
        [InlineData("$c$", "hello world", "hello+world")]
        [InlineData("$c$", "number 1", "number+1")]
        [InlineData("$c$", "hello ", "hello+")]
        [InlineData("$c$", "hello", "hello")]
        [InlineData("$c$", "hello `", "hello+%60")]
        [InlineData("URL: $c$", "un deux / \\ - <", "URL: un+deux+%2f+%5c+-+%3c")]
        //
        [InlineData("$C$", "", "")]
        [InlineData("$C$", null, "")]
        [InlineData("$C$", "hello world", "hello+world")]
        [InlineData("$C$", "number 1", "number+1")]
        [InlineData("$C$", "hello ", "hello+")]
        [InlineData("$C$", "hello", "hello")]
        [InlineData("$C$", "hello `", "hello+%60")]
        [InlineData("URL: $C$", "un deux / \\ - <", "URL: un+deux+%2f+%5c+-+%3c")]
        public void ReplaceWithWebClipboardText(string actual, string param, string expected)
        {
            var clipboard = Substitute.For<IClipboardService>();
            clipboard.GetText().Returns(param);

            var rpl = new WebClipboardReplacement(clipboard);

            rpl.Replace(actual, null)
               .Should().Be(expected);
        }

        [Theory]
        [InlineData("$w$", "", "")]
        [InlineData(null, "un deux", "")]
        [InlineData("", "un deux", "")]
        [InlineData("$w$", null, "")]
        [InlineData("$w$", "hello world", "hello+world")]
        [InlineData("$w$", "number 1", "number+1")]
        [InlineData("$w$", "hello ", "hello+")]
        [InlineData("$w$", "hello", "hello")]
        [InlineData("$w$", "hello `", "hello+%60")]
        [InlineData("URL: $w$", "un deux / \\ - <", "URL: un+deux+%2f+%5c+-+%3c")]
        //
        [InlineData("$W$", "", "")]
        [InlineData("$W$", null, "")]
        [InlineData("$W$", "hello world", "hello+world")]
        [InlineData("$W$", "number 1", "number+1")]
        [InlineData("$W$", "hello ", "hello+")]
        [InlineData("$W$", "hello", "hello")]
        [InlineData("$W$", "hello `", "hello+%60")]
        [InlineData("URL: $W$", "un deux / \\ - <", "URL: un+deux+%2f+%5c+-+%3c")]
        public void ReplaceWithWebText(string actual, string param, string expected)
        {
            var rpl = new WebTextReplacement();

            rpl.Replace(actual, param)
               .Should().Be(expected);
        }

        #endregion Methods
    }
}