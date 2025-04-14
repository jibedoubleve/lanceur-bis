using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Octokit;

namespace Lanceur.Ui.Core.Services;

public class GithubService : IGithubService
{
    #region Fields

    private readonly GitHubClient _client;
    private readonly ISettingsFacade _settingsFacade;
    private const string Owner = "jibedoubleve";
    private const string Repository = "lanceur-bis";

    #endregion

    #region Constructors

    public GithubService(ISettingsFacade settingsFacade)
    {
        _settingsFacade = settingsFacade;
        _client = new(new ProductHeaderValue("Lanceur"));
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task CreateIssue(string title)
    {
        if (!_settingsFacade.Application.Github.HasToken) return;

        var issue =  new NewIssue(title);
        _client.Credentials = new(_settingsFacade.Application.Github.Token);
        await _client.Issue.Create(Owner, Repository, issue);
    }

    /// <inheritdoc />
    public async Task<string> GetLatestVersion()
    {
        var info = await _client.Repository.Release.GetLatest(Owner, Repository);
        return info.TagName;
    }

    #endregion
}