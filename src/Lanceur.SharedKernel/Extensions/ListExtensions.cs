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

        foreach (var item in toRemove) source.Remove(item);
    }

    #endregion
}