using System.Reflection;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Lanceur.Ui.WPF.Services;

public class ReleaseService : IReleaseService
{
    #region Fields

    private readonly ILogger<ReleaseService> _logger;

    #endregion

    #region Constructors

    public ReleaseService(ILogger<ReleaseService> logger) => _logger = logger;

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task<(bool HasUpdate, Version Version)> HasUpdateAsync()
    {
        var currentVersion = CurrentVersion.FromAssembly(Assembly.GetExecutingAssembly());

        var client = new GitHubClient(new ProductHeaderValue("Lanceur"));
        var info = await client.Repository.Release.GetLatest("jibedoubleve", "lanceur-bis");

        _logger.LogInformation(
            "Application version is {AppVersion}, latest released version is {Tag}",
            currentVersion.Version,
            info.TagName
        );

        if (!Version.TryParse(info.TagName, out var version))
        {
            _logger.LogWarning("The tag of the version ('{Tag}') is not a valid version number", info.TagName);
            return (false, new());
        }

        return ConditionalExecution.Execute(
            () => (false, new Version()),
            () => (currentVersion.Version < version, version)
        );
    }

    #endregion
}