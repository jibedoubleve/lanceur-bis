using Lanceur.Core.Services;
using System.IO;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.PackagedApp
{
    public class PackagedAppSearchService : AbstractPackagedAppSearchService, IPackagedAppSearchService
    {
        private readonly ILogger<PackagedAppSearchService> _logger;

        public PackagedAppSearchService(ILoggerFactory factory)
        {
            _logger = factory.GetLogger<PackagedAppSearchService>();
        }
        #region Methods

        public async Task<IEnumerable<Core.Models.PackagedApp>> GetByInstalledDirectory(string fileName)
        {
            fileName = fileName.Replace("package:", "");
            var installedDir = Path.GetDirectoryName(fileName);

            return await Task.Run(() =>
            {
                var userPackages = GetUserPackages();
                return userPackages.AsParallel()
                                   .Where(p =>
                                   {
                                       try
                                       {
                                           return p is { IsFramework: false, IsDevelopmentMode: false }
                                                  && (p.InstalledLocation.Path == installedDir || p.IsAppUserModelId(fileName));
                                       }
                                       catch (Exception ex)
                                       {
                                           _logger.LogWarning(ex, "An error occured when selecting package {FileName}", fileName);
                                           return false;
                                       }
                                   })
                                   .Select(p => new Core.Models.PackagedApp
                                   {
                                       AppUserModelId    = p.GetAppUserModelId(),
                                       InstalledLocation = p.InstalledLocation.Path,
                                       Logo              = p.Logo,
                                       Description       = p.Description,
                                       DisplayName       = p.DisplayName
                                   })
                                   .Where(e => !string.IsNullOrEmpty(e.AppUserModelId))
                                   .ToArray();
            });
        }

        #endregion Methods
    }
}