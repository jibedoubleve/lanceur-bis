using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lanceur.Core.Models;

public class ObservableModel : INotifyPropertyChanged
{
    #region Events

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion Events

    #region Methods

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));

    protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) { return false; }
        else
        {
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    #endregion Methods
}