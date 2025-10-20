namespace Lanceur.Core.Configuration;

public static class GithubSectionExtensions
{
    #region Methods

    /// <summary>
    ///     Indicates whether the API token has been saved
    /// </summary>
    public static bool HasToken(this GithubSection current) => !string.IsNullOrEmpty(current.Token);

    #endregion
}

public class GithubSection
{
    public GithubSection()
    {
        Tag = "ungroomed";
    }
    #region Properties

    /// <summary>
    ///     Indicates the latest version released
    /// </summary>
    public Version LastCheckedVersion { get; set; }

    /// <summary>
    ///     If set at <c>True</c>, the next start of the application, the user will be notified if there's a new version
    ///     released
    ///     Otherwise, user won't be notified
    /// </summary>
    public bool SnoozeVersionCheck { get; set; }

    /// <summary>
    ///     Tag assigned to the issue when it is created via the @github_issue@ macro.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    ///     The token to use for actions that require privileges
    /// </summary>
    public string Token { get; set; }

    #endregion
}