namespace Lanceur.SharedKernel.Mixins;

public static class IndexNavigatorMixin
{
    #region Methods

    private static int CalculateNextIndex(this int current, int max)
    {
        if (current < -1 || current >= max)
            throw new IndexOutOfRangeException("Cannot navigate to next index when index if out of range.");
        else if (max == 0)
            throw new IndexOutOfRangeException("Cannot navigate to next index in empty collection.");
        else if (max == 1)
            return current <= -1 ? 0 : current;
        else if (current == max - 1)
            return 0;
        else
            return current + 1;
    }

    private static int CalculatePreviousIndex(this int current, int max)
    {
        if (current < -1 || current >= max)
            throw new IndexOutOfRangeException("Cannot navigate to previous index when index if out of range.");
        else if (max == 0)
            throw new IndexOutOfRangeException("Cannot navigate to previous index in empty collection.");
        else if (max == 1)
            return current <= -1 ? 0 : current;
        else if (new int[] { -1, 0 }.Contains(current))
            return max - 1;
        else
            return current - 1;
    }

    /// <summary>
    /// Indicates whether we can navigate through the collection.
    /// In other words, it indicates whether the collection is empty or not
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="collection">The collection to test</param>
    /// <returns>
    /// <c>True</c> if we can navigate (the collection has elements). Otherwise, <c>False</c>
    /// </returns>
    public static bool CanNavigate<T>(this IEnumerable<T> collection) => collection.Any();

    public static int GetNextIndex<T>(this IEnumerable<T> collection, int current) => current.CalculateNextIndex(collection.Count());

    /// <summary>
    /// Get the items next to the item defined by the specified index
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="collection">The collection to navigate</param>
    /// <param name="currentIndex">The actual index into the collection</param>
    /// <returns>The next element</returns>
    public static T GetNextItem<T>(this IEnumerable<T> collection, int currentIndex) => collection.ElementAt(collection.GetNextIndex(currentIndex));

    public static int GetPreviousIndex<T>(this IEnumerable<T> collection, int current) => current.CalculatePreviousIndex(collection.Count());

    /// <summary>
    /// Get the items previous to the item defined by the specified index
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="collection">The collection to navigate</param>
    /// <param name="currentIndex">The actual index into the collection</param>
    /// <returns>The next element</returns>
    public static T GetPreviousItem<T>(this IEnumerable<T> collection, int currentIndex) => collection.ElementAt(collection.GetPreviousIndex(currentIndex));

    #endregion Methods
}