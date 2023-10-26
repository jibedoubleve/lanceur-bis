namespace System.IO.FileOps.Core;

public interface IOperation
{
    #region Properties

    public Dictionary<string, string> Parameters { get; }

    #endregion Properties

    #region Methods

    Task ProcessAsync();

    #endregion Methods
}