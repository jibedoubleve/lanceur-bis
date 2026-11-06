using Lanceur.Core.Models;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Models;

public sealed class AdditionalParameterTest
{
    #region Methods

    /// <summary>
    ///     Regression test for issue #1420. Leading and trailing quotes are removed from the parameter,
    ///     quotes inside the parameter are kept.
    /// </summary>
    [Theory]
    [InlineData("foo", "foo")]
    [InlineData("\"foo\"", "foo")]
    [InlineData("\"foo", "foo")]
    [InlineData("foo\"", "foo")]
    [InlineData("\"\"foo\"\"", "foo")]
    [InlineData("  \"foo\"  ", "foo")]
    [InlineData("\" foo \"", "foo")]
    [InlineData("--title \"a b\" --x", "--title \"a b\" --x")]
    [InlineData("\"--title \"a b\" --x\"", "--title \"a b\" --x")]
    [InlineData("'foo'", "'foo'")]
    [InlineData("\"\"", "")]
    [InlineData("\"", "")]
    [InlineData("", "")]
    public void When_setting_parameter_Then_surrounding_quotes_are_removed(string parameter, string expected)
    {
        // ARRANGE & ACT
        var additionalParameter = new AdditionalParameter { Name = "name", Parameter = parameter };

        // ASSERT
        additionalParameter.Parameter.ShouldBe(expected);
    }

    [Fact]
    public void When_setting_null_parameter_Then_no_exception_is_thrown()
    {
        // ARRANGE
        var additionalParameter = new AdditionalParameter { Name = "name", Parameter = "foo" };

        // ACT
        var action = () => additionalParameter.Parameter = null!;

        // ASSERT
        action.ShouldNotThrow();
    }

    [Fact]
    public void When_setting_quoted_name_Then_name_is_trimmed()
    {
        // ARRANGE & ACT
        const string name = "aliasName";
        var additionalParameter = new AdditionalParameter { Name = $"\"{name}\"", Parameter = "foo" };

        // ASSERT
        additionalParameter.Name.ShouldBe(name);
    }

    [Fact]
    public void When_setting_quoted_version_of_same_parameter_Then_it_is_not_dirty()
    {
        // ARRANGE
        var additionalParameter = new AdditionalParameter { Name = "name", Parameter = "foo" };
        additionalParameter.MarkUnchanged();

        // ACT
        additionalParameter.Parameter = "\"foo\"";

        // ASSERT
        additionalParameter.IsDirty.ShouldBeFalse();
    }

    #endregion
}
