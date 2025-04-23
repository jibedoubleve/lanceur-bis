using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Extensions;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public class PackagedAppSearchService : AbstractPackagedAppSearchService, IPackagedAppSearchService
{
    #region Fields

    private readonly ILogger<PackagedAppSearchService> _logger;

    #endregion

    #region Constructors

    public PackagedAppSearchService(ILoggerFactory factory) => _logger = factory.GetLogger<PackagedAppSearchService>();

    #endregion

    #region Methods

    /// <inheritdoc />
    public async Task<IEnumerable<PackagedApp>> GetByInstalledDirectoryAsync(string fileName)
    {
        fileName = fileName.Replace("package:", "");
        var installedDir = fileName.GetDirectoryName();

        return await Task.Run(() =>
            {
                var userPackages = GetUserPackages();
                return userPackages.AsParallel()
                                   .Where(p =>
                                       {
                                           try { return p is { IsFramework: false, IsDevelopmentMode: false } && (installedDir.StartsWith(p.InstalledLocation.Path) || p.IsAppUserModelId(fileName)); }
                                           catch (Exception ex)
                                           {
                                               _logger.LogWarning(ex, "An error occured when selecting package {FileName}", fileName);
                                               return false;
                                           }
                                       }
                                   )
                                   .Select(p => new PackagedApp
                                       {
                                           AppUserModelId    = p.GetAppUserModelId(),
                                           InstalledLocation = p.InstalledLocation.Path,
                                           Logo              = p.Logo,
                                           Description       = p.Description,
                                           DisplayName       = p.DisplayName
                                       }
                                   )
                                   .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                                   .ToArray();
            }
        );
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PackagedApp>> GetInstalledUwpAppsAsync()
    {
        return await Task.Run(() =>
            {
                var packages = new List<PackagedApp>();
                foreach (var package in GetUserPackages())
                {
                    var appUserModelId = package.GetAppListEntries()
                                                .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                                                .Select(e => e.AppUserModelId)
                                                .FirstOrDefault(e => !string.IsNullOrEmpty(e));

                    if (appUserModelId.IsNullOrEmpty()) continue;

                    var currentPackage = new PackagedApp
                    {
                        AppUserModelId    = appUserModelId,
                        InstalledLocation = package.InstalledLocation.Path,
                        Logo              = package.Logo,
                        Description       = package.Description,
                        DisplayName       = package.DisplayName
                    };
                    packages.Add(currentPackage);
                }

                return packages;
            }
        );
    }

    /// <inheritdoc />
    public async Task<bool> TryResolveDetailsAsync(AliasQueryResult queryResult)
    {
        ArgumentNullException.ThrowIfNull(queryResult);
        var results = await GetByInstalledDirectoryAsync(queryResult.FileName);
        results  = results.ToArray();

        if (!results.Any()) return false;

        var result = results.First();
        if (queryResult.Description.IsNullOrEmpty()) queryResult.Description = result.DisplayName ?? "Packaged App";

        queryResult.FileName = result.FileName;
        return true;
    }

    #endregion
}