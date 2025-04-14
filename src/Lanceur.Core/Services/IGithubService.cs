namespace Lanceur.Core.Services;

public interface IGithubService
{
    #region Methods

    /// <summary>
    ///     Create a new issue on github with only a descriptive title
    /// </summary>
    /// <param name="title"></param>
    Task CreateIssue(string title);

    /// <summary>
    ///     Returns the latest version released
    /// </summary>
    /// <returns></returns>
    Task<string> GetLatestVersion();

    #endregion
}