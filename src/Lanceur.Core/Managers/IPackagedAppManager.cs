﻿namespace Lanceur.Core.Managers
{
    public interface IPackagedAppManager
    {
        #region Methods

        Task<string> GetPackageUniqueIdAsync(string fileName);

        Task<string> GetPackageUriAsync(string fileName);

        Task<bool> IsPackageAsync(string fileName);
        Task<string> GetIconAsync(string fileName);

        #endregion Methods
    }
}