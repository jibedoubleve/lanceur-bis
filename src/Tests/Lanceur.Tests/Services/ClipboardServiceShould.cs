using FluentAssertions;
using Lanceur.Core.Services;
using Lanceur.Infra.Wildcards;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Lanceur.Tests.Services;

public class ClipboardServiceShould
{
    #region Methods

    [Theory]
    [InlineData("$r$", "", "")]
    [InlineData("Hello $r$", "world", "Hello world")]
    [InlineData("Number $r$", "1", "Number 1")]
    [InlineData("Hello $r$", " ", "Hello  ")]
    [InlineData("Hello $r$", "", "Hello ")]
    [InlineData("Hello $r$", "`", "Hello `")]
    [InlineData("$c$", "", "")]
    [InlineData("$c$", "hello world", "hello+world")]
    [InlineData("$c$", "number 1", "number+1")]
    [InlineData("$c$", "hello ", "hello+")]
    [InlineData("$c$", "hello", "hello")]
    [InlineData("$c$", "hello `", "hello+%60")]
    [InlineData("URL: $c$", "un deux / \\ - <", "URL: un+deux+%2f+%5c+-+%3c")]
    [InlineData("$i$", "", "")]
    [InlineData("Hello $i$", "world", "Hello world")]
    [InlineData("Number $i$", "1", "Number 1")]
    [InlineData("Hello $i$", " ", "Hello  ")]
    [InlineData("Hello $i$", "", "Hello ")]
    [InlineData("Hello $i$", "`", "Hello `")]
    [InlineData("$w$", "", "")]
    [InlineData("$w$", "hello world", "hello+world")]
    [InlineData("$w$", "number 1", "number+1")]
    [InlineData("$w$", "hello ", "hello+")]
    [InlineData("$w$", "hello", "hello")]
    [InlineData("$w$", "hello `", "hello+%60")]
    [InlineData("URL: $w$", "un deux / \\ - <", "URL: un+deux+%2f+%5c+-+%3c")]
    public void ReplaceWithText(string actual, string param, string expected)
    {
        var clipboard = Substitute.For<IClipboardService>();
        var logger = Substitute.For<ILogger<ReplacementComposite>>();
        clipboard.RetrieveText().Returns(param);
        var mgr = new ReplacementComposite(clipboard, logger);

        mgr.Replace(actual, param)
           .Should()
           .Be(expected);
    }

    [Theory]
    [InlineData("", "", "")]
    [InlineData("", "un deux", "un deux")]
    [InlineData("un $i$ trois", "deux", "un deux trois")]
    public void ReturnParametersAsExpected(string aliasParam, string userParam, string expected)
    {
        var clipboard = Substitute.For<IClipboardService>();
        var logger = Substitute.For<ILogger<ReplacementComposite>>();
        var mgr = new ReplacementComposite(clipboard, logger);

        mgr.ReplaceOrReplacementOnNull(aliasParam, userParam).Should().Be(expected);
    }

    #endregion
}