using System.Security.Principal;
using Windows.Management.Deployment;

namespace Lanceur.Infra.Win32.PackagedApp;

public class AbstractPackagedAppSearchService
{
    #region Fields

    private SecurityIdentifier? _currentUser;
    private IEnumerable<Windows.ApplicationModel.Package>? _packages;

    #endregion Fields

    #region Methods

    private SecurityIdentifier? GetCurrentUser()
    {
        _currentUser ??= WindowsIdentity.GetCurrent().User;
        return _currentUser;
    }

    protected IEnumerable<Windows.ApplicationModel.Package> GetUserPackages()
    {
        if (_packages is not null) return _packages.ToArray();

        var user = GetCurrentUser();
        if (user is null) return Array.Empty<Windows.ApplicationModel.Package>();

        _packages = new PackageManager().FindPackagesForUser(user.Value);
        return _packages;
    }

    #endregion Methods
}