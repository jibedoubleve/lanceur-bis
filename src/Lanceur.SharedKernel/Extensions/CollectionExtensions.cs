using System.Collections.ObjectModel;

namespace Lanceur.SharedKernel.Extensions;

public static class CollectionExtensions
{
    #region Methods

    /// <summary>
    ///     Adds the elements of the specified collection to the end of the list.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements in the list and collection.
    /// </typeparam>
    /// <param name="list">
    ///     The list to which elements will be added.
    /// </param>
    /// <param name="items">
    ///     The collection of items to add to the list.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if either <paramref name="list" /> or <paramref name="items" /> is null.
    /// </exception>
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        foreach (var item in items) list.Add(item);
    }

    /// <summary>
    ///     Move the specified item to the specified index
    /// </summary>
    /// <param name="list">List of items where items will be moved</param>
    /// <param name="item">Item to move</param>
    /// <remarks>
    ///     https://stackoverflow.com/questions/450233/generic-list-moving-an-item-within-the-list
    /// </remarks>
    public static void Move<T>(this IList<T> list, T item, int newIndex)
    {
        if (item is not null)
        {
            var oldIndex = list.IndexOf(item);
            if (oldIndex >= 0)
            {
                list.RemoveAt(oldIndex);
                if (newIndex > oldIndex) newIndex--;
                list.Insert(newIndex, item);
            }
        }
    }

    public static IEnumerable<T> ToEnumerable<T>(this T item)
    {
        var collection = new List<T> { item };
        return collection;
    }

    #endregion
}