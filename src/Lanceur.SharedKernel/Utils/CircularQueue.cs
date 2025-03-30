using System.Collections;

namespace Lanceur.SharedKernel.Utils;

/// <summary>
///     Represents a circular queue with a fixed maximum size.
///     When the maximum size is reached, adding a new item will remove the oldest item.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public class CircularQueue<T> : IEnumerable<T>
{
    #region Fields

    private readonly int _maxSize;
    private readonly Queue<T> _queue;

    #endregion

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the CircularQueue class with the specified maximum size.
    /// </summary>
    /// <param name="maxSize">The maximum number of elements the queue can hold.</param>
    public CircularQueue(int maxSize)
    {
        _maxSize = maxSize;
        _queue = new(maxSize);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    ///     Adds an item to the queue. If the queue is at maximum capacity, the oldest item is removed first.
    /// </summary>
    /// <param name="item">The item to add to the queue.</param>
    public void Enqueue(T item)
    {
        if (_queue.Count >= _maxSize) _queue.Dequeue();

        _queue.Enqueue(item);
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

    #endregion
}