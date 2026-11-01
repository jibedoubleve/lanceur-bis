using Lanceur.Core.Services;
using Lanceur.Infra.Wildcards;
using Lanceur.Tests.Tools.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Services;

public sealed class ClipboardServiceTest
{
    #region Fields

    private readonly ITestOutputHelper _output;

    #endregion

    #region Constructors

    public ClipboardServiceTest(ITestOutputHelper output) => _output = output;

    #endregion

    #region Methods

    private ILogger<ReplacementComposite> CreateLogger()
        => new TestOutputHelperDecoratorForMicrosoftLogging<ReplacementComposite>(_output);

    [Theory]
    [InlineData("", "", "")]
    [InlineData("", "un deux", "un deux")]
    [InlineData("un $i$ trois", "deux", "un deux trois")]
    public void When_alias_parameters_are_empty_or_have_wildcard_Then_user_parameters_are_applied(
        string aliasParam, string userParam, string expected)
    {
        var clipboard = Substitute.For<IClipboardService>();
        var mgr = new ReplacementComposite(clipboard, CreateLogger());

        mgr.ReplaceOrReplacementOnNull(aliasParam, userParam).ShouldBe(expected);
    }

    #endregion
}