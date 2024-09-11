using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Utils;
using Lanceur.SharedKernel.Mixins;
using Microsoft.Extensions.Logging;
using Splat;

namespace Lanceur.Ui.Core.Utils.ConnectionStrings;

public class ConnectionString : BaseConnectionString, IConnectionString
{
    private readonly ILogger<ConnectionString> _logger;

    #region Fields

    private readonly string _dbPath;

    #endregion Fields

    #region Constructors

    // TODO: STG-Provide settings instead of service
    public ConnectionString(ILocalConfigRepository localConfiguration, ILogger<ConnectionString> logger)
    {
        ArgumentNullException.ThrowIfNull(localConfiguration);
        ArgumentNullException.ThrowIfNull(logger);

        if (localConfiguration?.Current.DbPath.IsNullOrWhiteSpace() ?? false) 
            throw new ArgumentNullException(nameof(localConfiguration.Current.DbPath), "Database path should have a value");

        _logger = logger;
        var s = localConfiguration!.Current;
        _dbPath = Environment.ExpandEnvironmentVariables(s.DbPath);
    }

    #endregion Constructors

    #region Methods

    public override string ToString()
    {
        if (!File.Exists(_dbPath)) _logger.LogWarning("The path {DbPath} doesn't exist. A new database should be created!", _dbPath);
        var path = Environment.ExpandEnvironmentVariables(_dbPath);
        return CSTRING_PATTERN.Format(path);
    }

    #endregion Methods
}