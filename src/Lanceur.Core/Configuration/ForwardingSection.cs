namespace Lanceur.Core.Configuration;

/// <summary>
///     An <see cref="ISection{T}" /> that delegates to an <see cref="IWriteableSection{T}" />,
///     ensuring both share the same underlying <see cref="Section{T}" /> instance.
/// </summary>
public sealed class ForwardingSection<T> : ISection<T> where T : class
{
    #region Fields

    private readonly IWriteableSection<T> _inner;

    #endregion

    #region Constructors

    public ForwardingSection(IWriteableSection<T> inner) => _inner = inner;

    #endregion

    #region Properties

    public T Value => _inner.Value;

    #endregion

    #region Methods

    public void Reload() => _inner.Reload();

    #endregion
}