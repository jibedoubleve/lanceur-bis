using System.Collections;
using System.Globalization;
using System.Windows.Data;
using Shouldly;
using Lanceur.Infra.Stores;
using Lanceur.Ui.WPF.Converters;
using Xunit;

namespace Lanceur.Tests.Ui;

public class ConverterShould
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

    #endregion

    private class DataConvertStoreTypeAsExpected : IEnumerable<object[]>
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