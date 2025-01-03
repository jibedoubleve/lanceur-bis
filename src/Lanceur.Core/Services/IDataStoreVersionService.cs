namespace Lanceur.Core.Services;

public interface IDataStoreVersionService
{
    #region Methods

    public Version GetCurrentDbVersion();

    /// <summary>
    /// Indicates whether the database is up to date and allowed
    /// to run with the current application.
    /// </summary>
    /// <param name="expectedVersion">
    /// Is the version the applications expects to run correctly
    /// </param>
    /// <returns><c>True</c> if version is ok; otherwise <c>False</c></returns>
    public bool IsUpToDate(Version expectedVersion);

    /// <summary>
    /// Indicates whether the database is up to date and allowed
    /// to run with the current application.
    /// </summary>
    /// <param name="goalVersion">
    /// Is the version the applications expects to run correctly
    /// </param>
    /// <returns><c>True</c> if version is ok; otherwise <c>False</c></returns>
    public bool IsUpToDate(string goalVersion);

    public void SetCurrentDbVersion(Version version);

    #endregion Methods
}