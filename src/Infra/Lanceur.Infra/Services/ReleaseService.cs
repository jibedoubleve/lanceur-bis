using System.Reflection;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Services;

public sealed class ReleaseService : IReleaseService
{
    #region Fields

    private readonly IGithubService _githubService;
    private readonly IUserGlobalNotificationService _notification;
    private readonly ISection<GithubSection> _githubSection;

    private readonly ILogger<ReleaseService> _logger;

    #endregion

    #region Constructors

    public ReleaseService(
        ILogger<ReleaseService> logger,
        IGithubService githubService,
        IUserGlobalNotificationService notification,
        ISection<GithubSection> githubSection)
    {
        _logger = logger;
        _githubService = githubService;
        _notification = notification;
        _githubSection = githubSection;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task CheckAndNotifyAsync()
    {
        var (hasUpdate, version) = await HasUpdateAsync();
        
        if (!hasUpdate || _githubSection.Value.SnoozeVersionCheck)
        {
            _logger.LogInformation(!hasUpdate
                ? "Application is up to date."
                : "Update available but notification is snoozed — skipping.");

            return;
        }

        // A new version has been release, notify user...
        
        _logger.LogDebug("New version available. Version {Version}", version);
        _notification.NotifyNewVersionAvailable(version);
    }

    private async Task<(bool HasUpdate, Version Version)> HasUpdateAsync()
    {
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
            return (false, new Version());
        }

        return ConditionalExecution.Execute(
            () => (false, new Version()),
            () => (currentVersion.Version < version, version)
        );
    }

    #endregion
}