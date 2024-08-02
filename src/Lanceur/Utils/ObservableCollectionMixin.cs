using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lanceur.Utils;

public static class ObservableCollectionMixin
{
    #region Methods

    public static void SortCollection<T, TKey>(this IList<T> collection, Func<T, TKey> selector)
    {
        // https://stackoverflow.com/a/39043757/389529
        var tempCollection = new List<T>(collection.OrderBy(selector));
        collection.Clear();
        foreach (var item in tempCollection) collection.Add(item);
    }

    #endregion Methods
}