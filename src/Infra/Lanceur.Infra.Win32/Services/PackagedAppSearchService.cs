﻿using System.IO;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.Infra.Win32.PackagedApp;
using Lanceur.SharedKernel.Mixins;
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
    public async Task<IEnumerable<Core.Models.PackagedApp>> GetByInstalledDirectory(string fileName)
    {
        fileName = fileName.Replace("package:", "");
        var installedDir = Path.GetDirectoryName(fileName);

        return await Task.Run(
            () =>
            {
                var userPackages = GetUserPackages();
                return userPackages.AsParallel()
                                   .Where(
                                       p =>
                                       {
                                           try { return p is { IsFramework: false, IsDevelopmentMode: false } && (p.InstalledLocation.Path == installedDir || p.IsAppUserModelId(fileName)); }
                                           catch (Exception ex)
                                           {
                                               _logger.LogWarning(ex, "An error occured when selecting package {FileName}", fileName);
                                               return false;
                                           }
                                       }
                                   )
                                   .Select(
                                       p => new Core.Models.PackagedApp
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
    public async Task<bool> TryResolveDetailsAsync(AliasQueryResult queryResult)
    {
        ArgumentNullException.ThrowIfNull(queryResult);
        var results = await GetByInstalledDirectory(queryResult.FileName);
        results  = results.ToArray();

        if (!results.Any()) return false;

        var result = results.First();
        if (queryResult.Notes.IsNullOrEmpty())
            queryResult.Description = result.Description.IsNullOrEmpty()
                ? result.DisplayName
                : result.Description;
        queryResult.FileName = result.FileName;
        return true;
    }

    #endregion
}