using System.ComponentModel;
using Lanceur.Core;
using Lanceur.Core.Configuration;
using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories.Config;
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

    private readonly IServiceProvider _serviceProvider;
    private readonly ISection<GithubSection> _settings;

    #endregion

    #region Constructors

    public GithubIssueMacro(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService<ILogger<GithubIssueMacro>>();
        _githubService = _serviceProvider.GetService<IGithubService>();
        _notification = serviceProvider.GetService<IUserGlobalNotificationService>();
        _enigma = serviceProvider.GetService<IEnigma>();
        _settings = _serviceProvider.GetSection<GithubSection>();
    }

    #endregion

    #region Properties

    public override string Icon => "Bug24";

    #endregion

    #region Methods

    public override SelfExecutableQueryResult Clone() => new GithubIssueMacro(_serviceProvider);

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