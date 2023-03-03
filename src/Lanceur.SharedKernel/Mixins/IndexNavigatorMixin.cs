namespace Lanceur.SharedKernel.Mixins
{
    public static class IndexNavigatorMixin
    {
        #region Methods

        public static bool CanNavigate<T>(this IEnumerable<T> collection) => collection.Any();

        public static T GetNextItem<T>(this IEnumerable<T> collection, int index) => collection.ElementAt(collection.GetNextIndex(index));

        public static T GetPreviousItem<T>(this IEnumerable<T> collection, int index) => collection.ElementAt(collection.GetPreviousIndex(index));

        private static int GetNextIndex(this int current, int max)
        {
            if (current < -1 || current >= max) { throw new IndexOutOfRangeException("Cannot navigate to next index when index if out of range."); }
            else if (max == 0) { throw new IndexOutOfRangeException("Cannot navigate to next index in empty collection."); }
            else if (max == 1) { return current <= -1 ? 0 : current; }
            else if (current == max - 1) { return 0; }
            else { return current + 1; }
        }

        public static int GetNextIndex<T>(this IEnumerable<T> collection, int current) => current.GetNextIndex(collection.Count());

        private static int GetPreviousIndex(this int current, int max)
        {
            if (current < -1 || current >= max) { throw new IndexOutOfRangeException("Cannot navigate to previous index when index if out of range."); }
            else if (max == 0) { throw new IndexOutOfRangeException("Cannot navigate to previous index in empty collection."); }
            else if (max == 1) { return current <= -1 ? 0 : current; }
            else if (new int[] { -1, 0 }.Contains(current)) { return max - 1; }
            else { return current - 1; }
        }

        public static int GetPreviousIndex<T>(this IEnumerable<T> collection, int current) => current.GetPreviousIndex(collection.Count());

        #endregion Methods
    }
}