using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Macros;

[Macro("github_issue")]
[Description("Create a new Github issue on lanceur-bis project")]
public class GithubIssueMacro : MacroQueryResult
{
    #region Fields

    private readonly IEnigma _enigma;

    private readonly IGithubService _githubService;
    private readonly ILogger<GithubIssueMacro> _logger;
    private readonly IUserGlobalNotificationService _notification;

    private readonly ISection<GithubSection> _settings;

    #endregion

    #region Constructors

    public GithubIssueMacro(
        ILogger<GithubIssueMacro> logger,
        IGithubService githubService,
        IUserGlobalNotificationService notification,
        IEnigma enigma,
        ISection<GithubSection> settings)
    {
        _logger = logger; 
        _githubService = githubService;
        _notification = notification;
        _enigma = enigma;
        _settings = settings;
    }

    #endregion

    #region Properties

    public override string Icon => "Bug24";

    #endregion

    #region Methods

    public override SelfExecutableQueryResult Clone() => new GithubIssueMacro(_logger, _githubService, _notification, _enigma, _settings);

    public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
    {
        if (cmdline == null || cmdline.IsNullOrEmpty() || !cmdline!.HasParameters)
        {
            _logger.LogInformation("The cmdline parameter is missing. Cannot create an issue on github.");
            return NoResult;
        }

        if (!_settings.Value.HasToken())
        {
            const string msg = "Cannot create an issue on github, no token is set.";
            _logger.LogInformation(msg);
            _notification.Warning(msg);
            return NoResult;
        }

        _logger.LogInformation("Creating Github issue with cmdline: {Cmdline}", cmdline!.ToString());
        await _githubService.CreateIssue(cmdline.Parameters, _enigma.Decrypt(_settings.Value.Token));

        return NoResult;
    }

    #endregion
}