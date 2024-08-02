using FluentAssertions;
using Lanceur.Converters;
using Lanceur.Infra.Formatters;
using NSubstitute;
using Splat;
using Xunit;

namespace Lanceur.Tests.Converters;

public class StringConverterShould
{
    #region Methods

    [Fact]
    public void FailWhenFormatterIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
            {
                var locator = Substitute.For<IReadonlyDependencyResolver>();
                new QueryDescriptionConverter(null, locator);
            }
        );
    }

    [Theory, InlineData("", ""), InlineData(null, null), InlineData(
         "111111111_222222222",
         "111111111_222222222"
     )]
    public void IndicateCorrectDescriptionWhenUsingDefaultFormatter(string input, string output)
    {
        var formatter = new DefaultStringFormatter();
        var converter = new QueryDescriptionConverter(formatter);

        var description = converter.Convert(input, null, null, null);

        description.Should().Be(output);
    }

    [Theory, InlineData(""), InlineData("111111111"), InlineData("111111111_222222222_333333333_444444444_555555555_666666666_777777777_888888888_999999999_000000000_111111111_12345"), InlineData("111111111_222222222_333333333_444444444_555555555_666666666_777777777_888888888_999999999_000000000_111111111_222222222_")]
    public void IndicateCorrectDescriptionWhenUsingLimitedStringFormatter(string input)
    {
        var formatter = new LimitedStringLengthFormatter();
        var converter = new QueryDescriptionConverter(formatter);

        var description = converter.Convert(input, null, null, null) as string;

        description.Should().NotBeNull();
        description!.Length.Should().BeLessOrEqualTo(LimitedStringLengthFormatter.MaxLength + 5);
    }

    #endregion Methods
}