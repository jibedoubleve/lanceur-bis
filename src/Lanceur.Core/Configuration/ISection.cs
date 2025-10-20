namespace Lanceur.Core.Configuration;

public interface ISection<out T>
{
    #region Properties

    T Value { get; }

    #endregion
}