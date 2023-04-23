using FluentAssertions;
using Lanceur.Converters;
using Lanceur.Infra.Formatters;
using Xunit;

namespace Lanceur.Tests.Converters
{
    public class StringConverterShould
    {
        #region Methods

        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData(
            "111111111_222222222",
            "111111111_222222222")
        ]
        public void IndicateCorrectDescriptionWhenUsingDefaultFormatter(string input, string output)
        {
            var formatter = new DefaultStringFormatter();
            var converter = new QueryDescriptionConverter(formatter);

            var description = converter.Convert(input, null, null, null);

            description.Should().Be(output);
        }

        [Theory]
        [InlineData("package:undeuxtrois", "Packaged Application")]
        [InlineData("", "")]
        [InlineData(
            "111111111_222222222_333333333_444444444_555555555_666666666_777777777_888888888_999999999_000000000_111111111_12345",
            "111111111_222222222_333333333_444444444_555555555_666666666_777777777_888888888_999999999_000000000_111111111_12345")
        ]
        [InlineData(
            "111111111_222222222_333333333_444444444_555555555_666666666_777777777_888888888_999999999_000000000_111111111_222222222_",
            "111111111_222222222_333333333_444444444_555555555_666666666_777777777_888888888_999999999_000000000_111111111_(...)")
        ]
        [InlineData(null, null)]
        public void IndicateCorrectDescriptionWhenUsingLimitedStringFormatter(string input, string output)
        {
            var formatter = new LimitedStringLengthFormatter();
            var converter = new QueryDescriptionConverter(formatter);

            var description = converter.Convert(input, null, null, null);

            description.Should().Be(output);
        }

        [Fact]
        public void FailWhenFormatterNotInIOC()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new QueryDescriptionConverter();

            });
        }

        [Fact]
        public void FailWhenFormatterIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new QueryDescriptionConverter(null);

            });
        }
        #endregion Methods
    }
}