using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lanceur.Core.Models;

public class ObservableModel : INotifyPropertyChanged
{
    #region Fields

    private bool _isDirty;

    /// <summary>
    ///     Properties that do not affect the dirty state when modified.
    ///     For example, changing <see cref="Count" /> on a clean item keeps it clean.
    /// </summary>
    private static readonly IEnumerable<string> ExcludedProperties = [
        nameof(IsDirty),
        nameof(AliasQueryResult.Thumbnail),
        nameof(AliasQueryResult.Count)
    ];

    #endregion

    #region Properties

    /// <summary>
    ///     Gets a value indicating whether the object has been modified since its creation or the last reset.
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        private set => SetField(ref _isDirty, value);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event to notify subscribers that a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    ///     The name of the property that changed. Automatically supplied by the compiler if not explicitly provided.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        => PropertyChanged?.Invoke(this, new(propertyName));

    /// <summary>
    ///     Updates the specified field with the provided value, triggers the <see cref="PropertyChanged" /> event
    ///     if the value has changed, and returns a boolean indicating whether the update occurred.
    /// </summary>
    /// <typeparam name="T">The type of the field and value.</typeparam>
    /// <param name="field">A reference to the field being updated.</param>
    /// <param name="value">The new value to assign to the field.</param>
    /// <param name="propertyName">
    ///     The name of the property associated with the field. Automatically supplied by the compiler if not explicitly
    ///     provided.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the field was updated and the <see cref="PropertyChanged" /> event was raised;
    ///     <c>false</c> if the field already contained the specified value.
    /// </returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        if (ExcludedProperties.Contains(propertyName)) return false;

        IsDirty = true;
        return true;
    }

    /// <summary>
    ///     Sets the <see cref="IsDirty" /> flag, marking the object as changed.
    /// </summary>
    public void MarkChanged() => IsDirty = true;

    /// <summary>
    ///     Resets the <see cref="IsDirty" /> flag, marking the object as unchanged.
    /// </summary>
    public void MarkUnchanged() => IsDirty = false;

    #endregion

    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion Events
}