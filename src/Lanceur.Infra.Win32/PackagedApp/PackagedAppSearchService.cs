﻿using Lanceur.Core.Services;
using System.IO;

namespace Lanceur.Infra.Win32.PackagedApp
{
    public class PackagedAppSearchService : AbstractPackagedAppSearchService, IPackagedAppSearchService
    {
        #region Methods

        public IEnumerable<Core.Models.PackagedApp> GetByInstalledDirectory(string fileName)
        {
            fileName = fileName.Replace("package:", "");
            var installedDir = Path.GetDirectoryName(fileName);

            return  GetUserPackages().AsParallel()
                                     .Where(p => p is { IsFramework: false, IsDevelopmentMode: false }
                                                 && (
                                                     p.InstalledLocation.Path == installedDir 
                                                     || p.IsAppUserModelId(fileName)))
                                     .Select(p => new Core.Models.PackagedApp
                                     {
                                         AppUserModelId    = p.GetAppUserModelId(),
                                         InstalledLocation = p.InstalledLocation.Path,
                                         Logo              = p.Logo
                                     })
                                     .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                                     .ToArray();
        }

        #endregion Methods
    }
}