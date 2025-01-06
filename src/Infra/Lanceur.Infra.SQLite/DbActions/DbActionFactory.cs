using Lanceur.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

internal class DbActionFactory : IDbActionFactory
{
    #region Fields

    private readonly IMappingService _converter;
    private readonly ILoggerFactory _loggerFactory;

    #endregion

    #region Constructors

    public DbActionFactory(IMappingService converter, ILoggerFactory loggerFactory)
    {
        _converter = converter;
        _loggerFactory = loggerFactory;
    }

    #endregion

    #region Properties

    public AliasDbAction AliasManagement => new(_loggerFactory);
    public AliasSaveDbAction SaveManagement => new(this, _loggerFactory);
    public AliasSearchDbAction SearchManagement => new(_loggerFactory, this);
    public MacroDbAction MacroManagement => new(_loggerFactory, _converter, this);
    public SetUsageDbAction UsageManagement => new(this);

    #endregion
}