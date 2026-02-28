namespace Lanceur.SharedKernel.Extensions;

public static class ListExtensions
{
    #region Methods

    /// <summary>
    ///     Removes multiple specified items from the provided list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="source">The list from which items will be removed. This must not be null.</param>
    /// <param name="toRemove">An enumerable collection of items to remove from the list. This must not be null.</param>
    /// <remarks>
    ///     This method iterates through each item in the <paramref name="toRemove" /> collection
    ///     and removes it from the <paramref name="source" /> list. If an item in <paramref name="toRemove" />
    ///     does not exist in <paramref name="source" />, no action is taken for that item.
    ///     It's important to ensure that <paramref name="source" /> and <paramref name="toRemove" />
    ///     are not null to avoid exceptions.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source" /> or <paramref name="toRemove" /> is null.</exception>
    public static void RemoveMultiple<T>(this IList<T> source, IEnumerable<T> toRemove)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(toRemove);

        var listToRemove = toRemove.ToList();

        //foreach (var item in toRemove) source.Remove(item);
        for (var i = 0; i < listToRemove.Count(); i++)
            if (source.Contains(listToRemove[i])) { source.Remove(listToRemove[i]); }
    }

    /// <summary>
    ///     Removes all items from the list that satisfy the specified predicate.
    /// </summary>
    /// <param name="source">The list from which elements will be removed.</param>
    /// <param name="predicate">A function that determines whether an element should be removed.</param>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public static void RemoveWhere<T>(this IList<T> source, Func<T, bool> predicate)
    {
        for (var i = 0; i < source.Count; ++i)
            if (predicate(source[i])) { source.RemoveAt(i); }
    }

    #endregion
}