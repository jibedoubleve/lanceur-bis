using Lanceur.Core.Responses;

namespace Lanceur.Core.Managers;

public interface IPackagedAppManager
{
    #region Methods

    Task<string> GetIconAsync(string fileName);

    Task<PackageResponse> GetPackageInfoAsync(string fileName);

    Task<string> GetPackageUniqueIdAsync(string fileName);

    Task<string> GetPackageUriAsync(string fileName);

    Task<bool> IsPackageAsync(string fileName);

    #endregion Methods
}