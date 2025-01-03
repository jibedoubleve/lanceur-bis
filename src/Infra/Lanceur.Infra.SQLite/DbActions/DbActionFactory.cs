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

    #region Methods

    public AliasDbAction AliasDbAction() => new(_loggerFactory);
    public AliasSearchDbAction AliasSearchDbAction() => new(_loggerFactory, this);
    public MacroDbAction MacroDbAction() => new(_loggerFactory, _converter, this);
    public SetUsageDbAction SetUsageDbAction() => new(_loggerFactory, this);

    #endregion
}