using System.Collections;
using System.Globalization;
using System.Windows.Data;
using Lanceur.Infra.Stores;
using Lanceur.Ui.WPF.Converters;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Ui.Converters;

public sealed class ConverterTest
{
    #region Methods

    [Theory]
    [InlineData("&")]
    [InlineData("!")]
    [InlineData(".")]
    public void When_ConvertBack_with_shortcut_Then_value_is_returned_unchanged(string shortcut)
    {
        // ARRANGE
        var converter = new StoreOrchestrationToStringConverter();

        // ACT
        var result = converter.ConvertBack(shortcut,
            typeof(string),
            null,
            CultureInfo.CurrentCulture) as string;

        // ASSERT - the stored regex must contain at least one capture group
        result.ShouldBe(shortcut);
    }

    [Theory]
    [InlineData("&")]
    [InlineData("!")]
    [InlineData(".")]
    public void When_Convert_after_ConvertBack_Then_original_shortcut_is_restored(string shortcut)
    {
        // ARRANGE
        var converter = new StoreOrchestrationToStringConverter();

        // ACT - round-trip
        var stored = converter.ConvertBack(shortcut,
            typeof(string),
            null,
            CultureInfo.CurrentCulture);
        var restored = converter.Convert(stored,
            typeof(string),
            null,
            CultureInfo.CurrentCulture) as string;

        // ASSERT
        restored.ShouldBe(shortcut);
    }

    [Theory]
    [InlineData(".")]
    [InlineData("")]
    [InlineData("m")]
    public void When_convert_back_value_Then_same_value_returned(string input)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.ConvertBack(input,
                     null!,
                     null,
                     null!)
                 .ShouldBe(input);
    }

    [Theory]
    [InlineData(@"^\s{0,}\..*", ".")]
    [InlineData(@"^\s{0,}.*", "")]
    [InlineData(@"^\s{0,}m.*", "m")]
    public void When_converting_old_regex_Then_shortcut_is_returned_for_retro_compatibility(string input, string output)
    {
        var converter = new StoreOrchestrationToStringConverter();

        converter.Convert(input,
                     null!,
                     null,
                     null!)
                 .ShouldBe(output);
    }

    [Theory]
    [ClassData(typeof(DataConvertStoreTypeAsExpected))]
    public void When_converting_store_type_Then_store_name_is_returned(object input, object output)
    {
        var converter = new StoreTypeToStoreNameConverter();
        converter.Convert(
                     input,
                     typeof(ConverterTest),
                     null,
                     CultureInfo.CurrentCulture
                 )
                 .ShouldBeEquivalentTo(output);
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