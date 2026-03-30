using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Lanceur.Infra.Stores;
using Lanceur.Ui.WPF.Converters;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Ui;

public sealed class ConverterShould
{
    #region Methods

    [Theory]
    [ClassData(typeof(DataConvertStoreTypeAsExpected))]
    public void ConvertStoreTypeAsExpected(object input, object output)
    {
        var converter = new StoreTypeToStoreNameConverter();
        converter.Convert(
                     input,
                     typeof(ConverterShould),
                     null,
                     CultureInfo.CurrentCulture
                 )
                 .ShouldBeEquivalentTo(output);
    }

    [Theory]
    [InlineData("&")]
    [InlineData("!")]
    [InlineData(".")]
    public void When_ConvertBack_with_shortcut_Then_value_is_returned_unchanged(string shortcut)
    {
        // ARRANGE
        var converter = new StoreOrchestrationToStringConverter();

        // ACT
        var result = converter.ConvertBack(shortcut, typeof(string), null, CultureInfo.CurrentCulture) as string;

        // ASSERT — the stored regex must contain at least one capture group
        result.ShouldBe(shortcut);
    }

    [Theory]
    [InlineData("&", "& half-life", "half-life")]
    [InlineData("&", "&", "")]
    [InlineData("!", "! foo", "foo")]
    public void When_ConvertBack_Then_capture_group_extracts_parameter(
        string shortcut, string fullQuery, string expectedCapture)
    {
        // ARRANGE
        var converter = new StoreOrchestrationToStringConverter();
        var pattern = converter.ConvertBack(shortcut, typeof(string), null, CultureInfo.CurrentCulture) as string;
        pattern.ShouldNotBeNull();

        // ACT
        var match = Regex.Match(fullQuery, pattern);

        // ASSERT — Groups[1] must contain the text after the shortcut
        match.Success.ShouldBeTrue();
        match.Groups[1].Value.Trim().ShouldBe(expectedCapture);
    }

    [Theory]
    [InlineData("&")]
    [InlineData("!")]
    [InlineData(".")]
    public void When_Convert_after_ConvertBack_Then_original_shortcut_is_restored(string shortcut)
    {
        // ARRANGE
        var converter = new StoreOrchestrationToStringConverter();

        // ACT — round-trip
        var stored  = converter.ConvertBack(shortcut, typeof(string), null, CultureInfo.CurrentCulture);
        var restored = converter.Convert(stored, typeof(string), null, CultureInfo.CurrentCulture) as string;

        // ASSERT
        restored.ShouldBe(shortcut);
    }

    #endregion

    private sealed class DataConvertStoreTypeAsExpected : IEnumerable<object[]>
    {
        #region Methods

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return ["Lanceur.Infra.Stores.BookmarksStore", "Bookmarks"];
            yield return ["BookmarksStore", "Bookmarks"];
            yield return [typeof(BookmarksStore), Binding.DoNothing];
        }

        #endregion
    }
}