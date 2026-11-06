using Lanceur.Core.Services;
using Lanceur.Tests.Tools;
using NSubstitute;
using Shouldly;
using Xunit;
using WildcardService = Lanceur.Infra.Services.WildcardService;

namespace Lanceur.Tests.Functional;

public sealed class ReplacementTest : TestBase
{
    #region Constructors

    public ReplacementTest(ITestOutputHelper outputHelper) : base(outputHelper) { }

    #endregion

    #region Methods

    [Theory]
    [InlineData("$r$", "", "")]
    [InlineData("", "un deux", "")]
    [InlineData("Hello $r$", "world", "Hello world")]
    [InlineData("Number $r$", "1", "Number 1")]
    [InlineData("Hello $r$", " ", "Hello  ")]
    [InlineData("Hello $r$", "", "Hello ")]
    [InlineData("Hello $r$", "`", "Hello `")]
    [InlineData("$R$", "", "")]
    [InlineData("Hello $R$", "world", "Hello world")]
    [InlineData("Number $R$", "1", "Number 1")]
    [InlineData("Hello $R$", " ", "Hello  ")]
    [InlineData("Hello $R$", "", "Hello ")]
    [InlineData("Hello $R$", "`", "Hello `")]
    public void When_using_clipboard_replacement_Then_wildcard_is_replaced_by_clipboard_text(
        string actual, string param, string expected)
    {
        var clipboard = Substitute.For<IClipboardService>();
        clipboard.RetrieveText().Returns(param);

        var rpl = new WildcardService(clipboard);

        rpl.Replace(actual, null)
           .ShouldBe(expected);
    }

    [Theory]
    [InlineData("$r$", "", "")]
    [InlineData("", "un deux", "")]
    [InlineData("Hello $r$", "world", "Hello world")]
    [InlineData("Number $r$", "1", "Number 1")]
    [InlineData("Hello $r$", " ", "Hello  ")]
    [InlineData("Hello $r$", "", "Hello ")]
    [InlineData("Hello $r$", "`", "Hello `")]
    [InlineData("$R$", "", "")]
    [InlineData("Hello $R$", "world", "Hello world")]
    [InlineData("Number $R$", "1", "Number 1")]
    [InlineData("Hello $R$", " ", "Hello  ")]
    [InlineData("Hello $R$", "", "Hello ")]
    [InlineData("Hello $R$", "`", "Hello `")]
    [InlineData("$i$", "", "")]
    [InlineData("Hello $i$", "world", "Hello world")]
    [InlineData("Number $i$", "1", "Number 1")]
    [InlineData("Hello $i$", " ", "Hello  ")]
    [InlineData("Hello $i$", "", "Hello ")]
    [InlineData("Hello $i$", "`", "Hello `")]
    [InlineData("$I$", "", "")]
    [InlineData("Hello $I$", "world", "Hello world")]
    [InlineData("Number $I$", "1", "Number 1")]
    [InlineData("Hello $I$", " ", "Hello  ")]
    [InlineData("Hello $I$", "", "Hello ")]
    [InlineData("Hello $I$", "`", "Hello `")]
    [InlineData("$c$", "", "")]
    [InlineData("$c$", "hello world", "hello+world")]
    [InlineData("$c$", "number 1", "number+1")]
    [InlineData("$c$", "hello ", "hello+")]
    [InlineData("$c$", "hello", "hello")]
    [InlineData("$c$", "hello `", "hello+%60")]
    [InlineData("URL: $c$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    [InlineData("$C$", "", "")]
    [InlineData("$C$", "hello world", "hello+world")]
    [InlineData("$C$", "number 1", "number+1")]
    [InlineData("$C$", "hello ", "hello+")]
    [InlineData("$C$", "hello", "hello")]
    [InlineData("$C$", "hello `", "hello+%60")]
    [InlineData("URL: $C$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    [InlineData("$w$", "", "")]
    [InlineData("$w$", "hello world", "hello+world")]
    [InlineData("$w$", "number 1", "number+1")]
    [InlineData("$w$", "hello ", "hello+")]
    [InlineData("$w$", "hello", "hello")]
    [InlineData("$w$", "hello `", "hello+%60")]
    [InlineData("URL: $w$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    [InlineData("$W$", "", "")]
    [InlineData("$W$", "hello world", "hello+world")]
    [InlineData("$W$", "number 1", "number+1")]
    [InlineData("$W$", "hello ", "hello+")]
    [InlineData("$W$", "hello", "hello")]
    [InlineData("$W$", "hello `", "hello+%60")]
    [InlineData("URL: $W$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    [InlineData("$w$-$i$-$w$-$i$", "a", "a-a-a-a")]
    [InlineData("$W$-$I$-$W$-$I$", "a", "a-a-a-a")]
    public void When_using_composite_replacement_Then_wildcards_are_replaced(
        string actual, string param, string expected)
    {
        var clipboard = Substitute.For<IClipboardService>();
        clipboard.RetrieveText().Returns(param);

        var rpl = new WildcardService(clipboard);

        rpl.Replace(actual, param)
           .ShouldBe(expected);
    }

    [Theory]
    [InlineData("$c$ hello world", true)]
    [InlineData("$r$ hello world", true)]
    [InlineData("$i$ hello world", true)]
    [InlineData("$w$ hello world", true)]
    [InlineData("$C$ hello world", true)]
    [InlineData("$R$ hello world", true)]
    [InlineData("$I$ hello world", true)]
    [InlineData("$W$ hello world", true)]
    [InlineData("hello world", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void When_using_templace_with_wildcards_Then_indicates_it_has_wildcards(string input, bool expected)
    {
        var rpl = new WildcardService(Substitute.For<IClipboardService>());

        rpl.HasWildcard(input).ShouldBe(expected);
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
    [InlineData("$I$", "", "")]
    [InlineData("$I$", null, "")]
    [InlineData("Hello $I$", "world", "Hello world")]
    [InlineData("Number $I$", "1", "Number 1")]
    [InlineData("Hello $I$", " ", "Hello  ")]
    [InlineData("Hello $I$", "", "Hello ")]
    [InlineData("Hello $I$", "`", "Hello `")]
    public void When_using_text_replacement_Then_wildcard_is_replaced_by_parameter(
        string actual, string param, string expected)
    {
        var rpl = new WildcardService(Substitute.For<IClipboardService>());

        rpl.Replace(actual, param)
           .ShouldBe(expected);
    }

    [Fact]
    public void When_using_unsupported_wildcard_Then_exception_is_thrown()
    {
        var rpl = new WildcardService(Substitute.For<IClipboardService>());

        Assert.Throws<NotSupportedException>(() => rpl.Replace("$Z$", "some value"));
    }

    [Theory]
    [InlineData("$c$", "", "")]
    [InlineData("", "un deux", "")]
    [InlineData("$c$", "hello world", "hello+world")]
    [InlineData("$c$", "number 1", "number+1")]
    [InlineData("$c$", "hello ", "hello+")]
    [InlineData("$c$", "hello", "hello")]
    [InlineData("$c$", "hello `", "hello+%60")]
    [InlineData("URL: $c$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    [InlineData("$C$", "", "")]
    [InlineData("$C$", "hello world", "hello+world")]
    [InlineData("$C$", "number 1", "number+1")]
    [InlineData("$C$", "hello ", "hello+")]
    [InlineData("$C$", "hello", "hello")]
    [InlineData("$C$", "hello `", "hello+%60")]
    [InlineData("URL: $C$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    public void When_using_web_clipboard_replacement_Then_wildcard_is_replaced_by_encoded_clipboard_text(
        string actual, string param, string expected)
    {
        var clipboard = Substitute.For<IClipboardService>();
        clipboard.RetrieveText().Returns(param);

        var rpl = new WildcardService(clipboard);

        rpl.Replace(actual, string.Empty)
           .ShouldBe(expected);
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
    [InlineData("URL: $w$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    [InlineData("$W$", "", "")]
    [InlineData("$W$", null, "")]
    [InlineData("$W$", "hello world", "hello+world")]
    [InlineData("$W$", "number 1", "number+1")]
    [InlineData("$W$", "hello ", "hello+")]
    [InlineData("$W$", "hello", "hello")]
    [InlineData("$W$", "hello `", "hello+%60")]
    [InlineData("URL: $W$", "un deux / \\ - <", "URL: un+deux+%2F+%5C+-+%3C")]
    public void When_using_web_text_replacement_Then_wildcard_is_replaced_by_encoded_parameter(
        string actual, string param, string expected)
    {
        var rpl = new WildcardService(Substitute.For<IClipboardService>());

        rpl.Replace(actual, param)
           .ShouldBe(expected);
    }

    #endregion
}