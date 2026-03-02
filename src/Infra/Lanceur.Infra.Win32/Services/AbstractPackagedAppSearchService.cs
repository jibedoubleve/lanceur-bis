using System.Security.Principal;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace Lanceur.Infra.Win32.Services;

public class AbstractPackagedAppSearchService
{
    #region Fields

    private SecurityIdentifier? _currentUser;
    private IEnumerable<Package>? _packages;

    #endregion

    #region Methods

    private SecurityIdentifier? GetCurrentUser()
    {
        _currentUser ??= WindowsIdentity.GetCurrent().User;
        return _currentUser;
    }

    protected IEnumerable<Package> GetUserPackages()
    {
        if (_packages is not null) { return _packages.ToArray(); }

        var user = GetCurrentUser();
        if (user is null) { return []; }

        return _packages ??= new PackageManager().FindPackagesForUser(user.Value)
                                                 .Where(e => e is not null)
                                                 .OrderBy(e => e.DisplayName);
    }

    #endregion
}