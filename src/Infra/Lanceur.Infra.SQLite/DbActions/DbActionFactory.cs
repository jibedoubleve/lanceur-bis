using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

internal class DbActionFactory : IDbActionFactory
{
    #region Fields

    private readonly ILoggerFactory _loggerFactory;

    #endregion

    #region Constructors

    public DbActionFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    #endregion

    #region Properties

    public AliasDbAction AliasManagement => new(_loggerFactory);
    public AliasSaveDbAction SaveManagement => new(this, _loggerFactory);
    public AliasSearchDbAction SearchManagement => new(_loggerFactory, this);
    public MacroDbAction MacroManagement => new(_loggerFactory, this);
    public SetUsageDbAction UsageManagement => new(this);

    #endregion
}