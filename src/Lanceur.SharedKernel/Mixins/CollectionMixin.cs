using System.Collections.ObjectModel;

namespace Lanceur.SharedKernel.Mixins
{
    public static class CollectionMixin
    {
        #region Methods

        /// <summary>
        /// Move the specified item to the specified index
        /// </summary>
        /// <param name="list">List of items where items will be moved</param>
        /// <param name="item">Item to move</param>
        /// <remarks>
        /// https://stackoverflow.com/questions/450233/generic-list-moving-an-item-within-the-list
        /// </remarks>
        public static void Move<T>(this IList<T> list, T item, int newIndex)
        {
            if (item is not null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex >= 0)
                {
                    list.RemoveAt(oldIndex);
                    if (newIndex > oldIndex) { newIndex--; }
                    list.Insert(newIndex, item);
                }
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable) => new(enumerable);

        #endregion Methods
    }
}