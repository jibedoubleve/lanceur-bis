using Lanceur.Core.Managers;
using Lanceur.Core.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Lanceur.Utils.PackagedApps
{
    public static class PackageMixin
    {
        #region Methods

        public static bool HasInstallationPath(this Package package) => Directory.Exists(package.InstalledPath);

        public static bool IsInDirectory(this Package package, string directory) => directory?.Contains(package.InstalledPath) ?? false;

        #endregion Methods
    }

    public class PackagedAppManager : IPackagedAppManager
    {
        #region Fields

        private static readonly Dictionary<string, Package> _cache = new();

        #endregion Fields

        #region Enums

        public enum PackageVersion
        {
            Windows10,
            Windows81,
            Windows8,
            Unknown
        }

        #endregion Enums

        #region Methods

        private static async Task<Package> GetPackageAsync(string fileName)
        {
            return await Task.Run(() =>
            {
                if (_cache.TryGetValue(fileName, out var expression)) { return expression; }

                var userId = WindowsIdentity.GetCurrent()?.User?.Value
                             ?? throw new NullReferenceException("Didn't find current Windows user!");

                var srcDir = Path.GetDirectoryName(fileName);

                var packages = new PackageManager().FindPackagesForUser(userId);
                var results = (from p in packages
                               where p.HasInstallationPath()
                                     && p.IsInDirectory(srcDir)
                               select p).ToList();

                var currentPkg = results.FirstOrDefault()
                    ?? throw new NullReferenceException($"No package for '{fileName}' found for the current user");

                _cache.Add(fileName, currentPkg);
                return currentPkg;
            });
        }

        public async Task<string> GetIconAsync(string fileName)
        {
            var currentPackage = await GetPackageAsync(fileName);

            var result = currentPackage != null
                ? PackageInformation.GetIcon(currentPackage)
                : string.Empty;

            return result;
        }

        public async Task<PackageResponse> GetPackageInfoAsync(string fileName)
        {
            var uid = await GetPackageUniqueIdAsync(fileName);

            return new PackageResponse(uid, fileName);
        }

        public async Task<string> GetPackageUniqueIdAsync(string fileName)
        {
            var currentPackage = await GetPackageAsync(fileName);

            var result = currentPackage != null
                ? PackageInformation.GetAppUserModelId(currentPackage)
                : string.Empty;

            return result;
        }

        public async Task<string> GetPackageUriAsync(string fileName) => $"package:{await GetPackageUniqueIdAsync(fileName)}";

        public async Task<bool> IsPackageAsync(string fileName) => await GetPackageUniqueIdAsync(fileName) != string.Empty;

        #endregion Methods
    }
}