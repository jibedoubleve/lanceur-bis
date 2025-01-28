using Lanceur.Core.Utils;
using Lanceur.Infra.Constants;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Ui.Core.Utils.ConnectionStrings;

public class LightConnectionString : BaseConnectionString, IConnectionString
{
    #region Fields

    private static string? _connectionString;

    private readonly string _dbPath = Paths.DefaultDb;

    #endregion Fields

    #region Constructors

    private LightConnectionString(string dbPath) => _dbPath = dbPath;

    #endregion Constructors

    #region Methods

    public static LightConnectionString FromFile(string dbPath) => new(dbPath);

    public override string ToString()
    {
        if (_connectionString is not null) return _connectionString;

        var path = _dbPath.ExpandPath();
        _connectionString = CSTRING_PATTERN.Format(path);

        return _connectionString;
    }

    #endregion Methods
}