namespace Lanceur.Core.Models.Settings;

public class GithubSection
{
    #region Properties

    /// <summary>
    ///     Indicates whether the API token has been saved
    /// </summary>
    public bool HasToken => !string.IsNullOrEmpty(Token);

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
    ///     The token to use for actions that require privileges
    /// </summary>
    public string Token { get; set; }

    #endregion
}