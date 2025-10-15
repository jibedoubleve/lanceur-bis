using Shouldly;
using Lanceur.SharedKernel.Extensions;
using Xunit;

namespace Lanceur.Tests.SharedKernel;

public class StringExtensionsShould
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
        s.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void ReturnFalseOnEmtyWhenUsingNullOrEmpty()
    {
        var s = string.Empty;
        s.ShouldBeNullOrEmpty();
    }

    [Theory, InlineData("tostring", "tostring"), InlineData("tostring", "TOSTRING"), InlineData("tostring", "ToString"), InlineData("", ""), InlineData(null, null)]
    public void ReturnLowerString(string expected, string actual) { actual.ToLowerString().ShouldBe(expected); }

    [Theory, InlineData("firstvalue", EnumValues.FirstValue), InlineData("secondvalue", EnumValues.SecondValue), InlineData("thirdvalue", EnumValues.ThirdValue), InlineData("fourthvalue", EnumValues.FourthValue)]
    public void ReturnLowerStringFromEnum(string expected, EnumValues actual) { actual.ToLowerString().ShouldBe(expected); }

    [Fact]
    public void ReturnTrueOnEmptyUsingNullOrWhiteSpace()
    {
        var s = string.Empty;

        s.ShouldBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReturnTrueOnNullUsingNullOrEmpty()
    {
        string s = null;
        s.ShouldBeNullOrEmpty();
    }

    [Fact]
    public void ReturnTrueOnNullUsingNullOrWhiteSpace()
    {
        string s = null;
        s.ShouldBeNullOrWhiteSpace();
    }

    [Theory, InlineData("un"), InlineData("deux")]
    public void ReturnTrueOnTextUsingNullOrEmpty(string value) { value.ShouldNotBeNullOrEmpty(); }

    [Theory, InlineData("un"), InlineData("deux")]
    public void ReturnTrueOnTextUsingNullOrWhiteSpace(string value) { value.ShouldNotBeNullOrWhiteSpace(); }

    #endregion Methods
}