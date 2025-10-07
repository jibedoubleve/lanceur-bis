using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Utils;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.Utils.ConnectionStrings;

public class ConnectionString : BaseConnectionString, IConnectionString
{
    private readonly ILogger<ConnectionString> _logger;

    #region Fields

    private readonly string _dbPath;

    #endregion Fields

    #region Constructors

    // TODO: STG-Provide settings instead of service
    public ConnectionString(IApplicationConfigurationService applicationConfiguration, ILogger<ConnectionString> logger)
    {
        ArgumentNullException.ThrowIfNull(applicationConfiguration);
        ArgumentNullException.ThrowIfNull(logger);

        if (applicationConfiguration?.Current.DbPath.IsNullOrWhiteSpace() ?? false) 
            throw new ArgumentNullException(nameof(applicationConfiguration.Current.DbPath), "Database path should have a value");

        _logger = logger;
        var s = applicationConfiguration!.Current;
        _dbPath = s.DbPath.ExpandPath();
    }

    #endregion Constructors

    #region Methods

    public override string ToString()
    {
        if (!File.Exists(_dbPath)) _logger.LogWarning("The path {DbPath} doesn't exist. A new database should be created!", _dbPath);
        var path = _dbPath.ExpandPath();
        return ConnectionStringPattern.Format(path);
    }

    #endregion Methods
}