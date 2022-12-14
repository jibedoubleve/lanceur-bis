using Lanceur.Core.Managers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Lanceur.Utils.PackagedApps
{
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

        private static string GetInstallationPath(Package package)
        {
            var path = package.InstalledLocation.Path;
            return path;
        }

        private static async Task<Package> GetPackageAsync(string fileName)
        {
            return await Task.Run(() =>
            {
                if (_cache.ContainsKey(fileName)) { return _cache[fileName]; }
                else
                {
                    var userId = WindowsIdentity.GetCurrent().User.Value;
                    var srcDir = Path.GetDirectoryName(fileName);

                    var packages = new PackageManager().FindPackagesForUser(userId);
                    var results = (from p in packages
                                   where HasInstallationPath(p)
                                      && srcDir.StartsWith(GetInstallationPath(p))
                                   select p).ToList();

                    var currentpackage = results.FirstOrDefault();
                    _cache.Add(fileName, currentpackage);
                    return currentpackage;
                }
            });
        }

        private static bool HasInstallationPath(Package package)
        {
            try
            {
                var res = package.InstalledLocation.Path;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetIconAsync(string fileName)
        {
            var currentPackage = await GetPackageAsync(fileName);

            var result = currentPackage != null
                ? PackageInformation.GetIcon(currentPackage)
                : string.Empty;

            return result;
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