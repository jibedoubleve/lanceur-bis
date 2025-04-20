namespace Lanceur.Core.Services;

public interface IGithubService
{
    #region Methods

    /// <summary>
    ///     Creates a new issue on GitHub with the specified title.
    /// </summary>
    /// <param name="title">The title of the issue to create.</param>
    /// <param name="token">The personal access token used for authentication with the GitHub API.</param>
    Task CreateIssue(string title, string token);

    /// <summary>
    ///     Retrieves the latest version tag from the GitHub repository's releases.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the latest version as a string.</returns>
    Task<string> GetLatestVersion();

    #endregion
}