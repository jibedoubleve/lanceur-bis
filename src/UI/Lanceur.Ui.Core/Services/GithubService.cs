using Lanceur.Core.Configuration;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Octokit;

namespace Lanceur.Ui.Core.Services;

public class GithubService : IGithubService
{
    #region Fields

    private readonly GitHubClient _client;
    private readonly IUserGlobalNotificationService _notificationService;
    private readonly string _tag;
    private const string GithubUrl = "https://github.com/jibedoubleve/lanceur-bis/issues/";
    private const string Owner = "jibedoubleve";
    private const string Repository = "lanceur-bis";

    #endregion

    #region Constructors

    public GithubService(ISection<GithubSection> settings, IUserGlobalNotificationService notificationService)
    {
        _notificationService = notificationService;
        _client = new(new ProductHeaderValue("Lanceur"));
        _tag = settings.Value.Tag;
    }

    #endregion

    #region Methods

    private static string Url(int number) => $"{GithubUrl}{number}";

    /// <inheritdoc />
    public async Task CreateIssue(string title, string token)
    {
        var issue =  new NewIssue(title) { Labels = { _tag } };
        _client.Credentials = new(token);
        var number = (await _client.Issue.Create(Owner, Repository, issue)).Number;
        _notificationService.InformationWithNavigation($"Created new Github issue n° {number}", Url(number));
    }

    /// <inheritdoc />
    public async Task<string> GetLatestVersion()
    {
        var info = await _client.Repository.Release.GetLatest(Owner, Repository);
        return info.TagName;
    }

    #endregion
}