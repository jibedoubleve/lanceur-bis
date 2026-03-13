using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Configurations;
using Lanceur.Core.Configuration.Sections.Infrastructure;
using Lanceur.Core.Services;
using Lanceur.Core.Utils;
using Lanceur.SharedKernel.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.Utils.ConnectionStrings;

public class ConnectionString : BaseConnectionString, IConnectionString
{
    #region Fields

    private readonly string _dbPath;
    private readonly ILogger<ConnectionString> _logger;

    #endregion

    #region Constructors

    // TODO: STG-Provide settings instead of service
    public ConnectionString(
        ISection<DatabaseSection> databaseSection,
        ILogger<ConnectionString> logger)
    {
        ArgumentNullException.ThrowIfNull(databaseSection);
        ArgumentNullException.ThrowIfNull(logger);

        if (databaseSection.Value.DbPath.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(
                nameof(databaseSection.Value.DbPath),
                "Database path should have a value"
            );
        }

        _logger = logger;
        var s = databaseSection.Value;
        _dbPath = s.DbPath.ExpandPath();
    }

    #endregion

    #region Methods

    public override string ToString()
    {
        if (!File.Exists(_dbPath))
        {
            _logger.LogWarning("The path {DbPath} doesn't exist. A new database should be created!", _dbPath);
        }

        var path = _dbPath.ExpandPath();
        return ConnectionStringPattern.Format(path);
    }

    #endregion
}