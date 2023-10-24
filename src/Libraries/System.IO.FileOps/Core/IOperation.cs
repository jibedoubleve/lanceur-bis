namespace System.IO.FileOps.Core;

public interface IOperation
{
    #region Public properties

    public Dictionary<string, string> Parameters { get; }

    #endregion

    #region Public methods

    Task ProcessAsync();

    #endregion
}