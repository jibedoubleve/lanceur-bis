using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Lanceur.Views;

public class QueryViewModel : ReactiveObject
{
    #region Constructors

    private QueryViewModel(string value, bool isActive) => (Value, IsActive) = (value, isActive);

    #endregion Constructors

    #region Properties

    public static QueryViewModel Empty => new(string.Empty, true);

    [Reactive] public bool IsActive { get; set; }

    [Reactive] public string Value { get; set; }

    #endregion Properties

    #region Methods

    public static implicit operator string(QueryViewModel query) => query.Value;

    public string Trim() => Value.Trim();

    #endregion Methods
}