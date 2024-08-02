using FluentAssertions;
using Lanceur.SharedKernel.Mixins;
using Xunit;

namespace Lanceur.Tests.SharedKernel;

public class StringMixinShould
{
    #region Enums

    public enum EnumValues
    {
        FirstValue,
        SecondValue,
        ThirdValue,
        FourthValue
    }

    #endregion Enums

    #region Methods

    [Fact]
    public void ReturnFalseOnEmtyUsingNullOrWhiteSpace()
    {
        var s = string.Empty;
        s.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ReturnFalseOnEmtyWhenUsingNullOrEmpty()
    {
        var s = string.Empty;
        s.Should().BeNullOrEmpty();
    }

    [Theory, InlineData("tostring", "tostring"), InlineData("tostring", "TOSTRING"), InlineData("tostring", "ToString"), InlineData("", ""), InlineData(null, null)]
    public void ReturnLowerString(string expected, string actual) { actual.ToLowerString().Should().Be(expected); }

    [Theory, InlineData("firstvalue", EnumValues.FirstValue), InlineData("secondvalue", EnumValues.SecondValue), InlineData("thirdvalue", EnumValues.ThirdValue), InlineData("fourthvalue", EnumValues.FourthValue)]
    public void ReturnLowerStringFromEnum(string expected, EnumValues actual) { actual.ToLowerString().Should().Be(expected); }

    [Fact]
    public void ReturnTrueOnEmptyUsingNullOrWhiteSpace()
    {
        var s = string.Empty;

        s.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    public void ReturnTrueOnNullUsingNullOrEmpty()
    {
        string s = null;
        s.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ReturnTrueOnNullUsingNullOrWhiteSpace()
    {
        string s = null;
        s.Should().BeNullOrWhiteSpace();
    }

    [Theory, InlineData("un"), InlineData("deux")]
    public void ReturnTrueOnTextUsingNullOrEmpty(string value) { value.Should().NotBeNullOrEmpty(); }

    [Theory, InlineData("un"), InlineData("deux")]
    public void ReturnTrueOnTextUsingNullOrWhiteSpace(string value) { value.Should().NotBeNullOrWhiteSpace(); }

    #endregion Methods
}