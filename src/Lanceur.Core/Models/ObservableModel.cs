using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lanceur.Core.Models;

public class ObservableModel : INotifyPropertyChanged
{
    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion Events

    #region Methods

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event to notify subscribers that a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically supplied by the compiler if not explicitly provided.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));

    /// <summary>
    /// Updates the specified field with the provided value, triggers the <see cref="PropertyChanged"/> event 
    /// if the value has changed, and returns a boolean indicating whether the update occurred.
    /// </summary>
    /// <typeparam name="T">The type of the field and value.</typeparam>
    /// <param name="field">A reference to the field being updated.</param>
    /// <param name="value">The new value to assign to the field.</param>
    /// <param name="propertyName">
    /// The name of the property associated with the field. Automatically supplied by the compiler if not explicitly provided.
    /// </param>
    /// <returns>
    /// <c>true</c> if the field was updated and the <see cref="PropertyChanged"/> event was raised; 
    /// <c>false</c> if the field already contained the specified value.
    /// </returns>
    protected bool SetField<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) { return false; }

        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion Methods
}