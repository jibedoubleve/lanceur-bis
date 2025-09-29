using System.Reflection;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public class ReleaseService : IReleaseService
{
    #region Fields

    private readonly ILogger<ReleaseService> _logger;
    private readonly IGithubService _githubService;

    #endregion

    #region Constructors

    public ReleaseService(ILogger<ReleaseService> logger, IGithubService githubService)
    {
        _logger = logger;
        _githubService = githubService;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task<(bool HasUpdate, Version Version)> HasUpdateAsync()
    {
        using var measure = TimeMeter.Measure(this, _logger);
        var currentVersion = CurrentVersion.FromAssembly(Assembly.GetExecutingAssembly());

        var tag = await _githubService.GetLatestVersion();
        _logger.LogInformation(
            "Application version is {AppVersion}, latest released version is {Tag}",
            currentVersion.Version,
            tag
        );

        if (!Version.TryParse(tag, out var version))
        {
            _logger.LogWarning("The tag {Tag} is not a valid version number", tag);
            return (false, new());
        }

        return ConditionalExecution.Execute(
            () => (false, new Version()),
            () => (currentVersion.Version < version, version)
        );
    }

    #endregion
}