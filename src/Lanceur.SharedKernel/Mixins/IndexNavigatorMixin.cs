namespace Lanceur.SharedKernel.Mixins;

public static class IndexNavigatorMixin
{
    #region Methods

    private static int ValidateNewIndex(int index, int max) => max >= 0 ? (max + index) % max : -1;

    /// <summary>
    ///     Indicates whether we can navigate through the collection.
    ///     In other words, it indicates whether the collection is empty or not
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="collection">The collection to test</param>
    /// <returns>
    ///     <c>True</c> if we can navigate (the collection has elements). Otherwise, <c>False</c>
    /// </returns>
    public static bool CanNavigate<T>(this IEnumerable<T> collection) => collection.Any();

    public static int GetNextIndex<T>(this IEnumerable<T> collection, int current)
    {
        if (current == -1) return -1;

        var max = collection.Count();
        return  current < -1 || current >= max
            ? throw new IndexOutOfRangeException("Cannot navigate to next index when index if out of range.")
            : ValidateNewIndex(current + 1, max);
    }

    /// <summary>
    ///     Retrieves the item following the one at the specified index in the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to navigate through.</param>
    /// <param name="currentIndex">The current index of the item in the collection.</param>
    /// <returns>The next item in the collection, based on the current index.</returns>
    public static T GetNextItem<T>(this IEnumerable<T> collection, int currentIndex) => collection.ElementAt(collection.GetNextIndex(currentIndex));

    public static int GetNextPage<T>(this IEnumerable<T> collection, int current, int pageSize)
    {
        if (current == -1) return -1;

        var max = collection.Count();
        var newIdx = current + pageSize > max 
            ? 0 
            : current + pageSize;
        
        return  current < -1 || current >= max
            ? throw new IndexOutOfRangeException("Cannot navigate to next page when index if out of range.")
            : ValidateNewIndex(newIdx, max);
    }

    /// <summary>
    ///     Calculates the previous index in the collection based on the current index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to navigate through.</param>
    /// <param name="current">The current index of the item in the collection.</param>
    /// <returns>The index of the previous item in the collection.</returns>
    public static int GetPreviousIndex<T>(this IEnumerable<T> collection, int current)
    {
        if (current == -1) return -1;

        var max = collection.Count();
        return current < 0 || current >= max
            ? throw new IndexOutOfRangeException("Cannot navigate to previous index when index if out of range.")
            : ValidateNewIndex(current - 1, max);
    }

    /// <summary>
    ///     Get the items previous to the item defined by the specified index
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="collection">The collection to navigate</param>
    /// <param name="currentIndex">The actual index into the collection</param>
    /// <returns>The next element</returns>
    public static T GetPreviousItem<T>(this IEnumerable<T> collection, int currentIndex) => collection.ElementAt(collection.GetPreviousIndex(currentIndex));

    public static int GetPreviousPage<T>(this IEnumerable<T> collection, int current, int pageSize)
    {
        if (current == -1) return -1;

        var max = collection.Count();
        var newIdx = current - pageSize < 0 
            ? max - 1 
            : current - pageSize;

        return  current < 0 || current >= max
            ? throw new IndexOutOfRangeException("Cannot navigate to previous page when index if out of range.")
            : ValidateNewIndex(newIdx, max);
    }

    #endregion
}