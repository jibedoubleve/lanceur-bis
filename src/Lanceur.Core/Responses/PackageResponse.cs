namespace Lanceur.Core.Responses;

public class PackageResponse
{
    #region Constructors

    public PackageResponse(string uid, string fileName)
    {
        UniqueId = uid;
        FileName = fileName;
    }

    #endregion Constructors

    #region Properties

    public string FileName { get; }
    public bool IsPackage => !string.IsNullOrEmpty(UniqueId);
    public string UniqueId { get; }
    public string Uri => IsPackage ? $"package:{UniqueId}" : FileName;

    #endregion Properties
}