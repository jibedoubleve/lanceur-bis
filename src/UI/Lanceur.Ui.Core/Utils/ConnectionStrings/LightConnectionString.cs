using Lanceur.Core.Utils;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Ui.Core.Utils.ConnectionStrings;

public class LightConnectionString : BaseConnectionString, IConnectionString
{
    #region Fields

    private readonly string _connectionString;

    #endregion

    #region Constructors

    private LightConnectionString(string dbPath)
    {
        ArgumentNullException.ThrowIfNull(dbPath);
        if (!Path.Exists(dbPath))
        {
            throw new InvalidDataException(
                "The path of the SQLite database does not exist or is invalid."
            );
        }

        var path = dbPath.ExpandPath();
        _connectionString = ConnectionStringPattern.Format(path);
    }

    #endregion

    #region Methods

    public static LightConnectionString FromFile(string dbPath) => new(dbPath);

    public override string ToString() => _connectionString;

    #endregion
}