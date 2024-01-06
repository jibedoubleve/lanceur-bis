using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Lanceur.SharedKernel.Utils;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public class AliasSearchDbAction
{
    #region Fields

    private readonly IDbConnectionManager _db;
    private readonly ILogger<AliasSearchDbAction> _logger;
    private readonly MacroDbAction _macroManager;

    #endregion Fields

    #region Constructors

    public AliasSearchDbAction(IDbConnectionManager db, ILoggerFactory logFactory, IConvertionService converter)
    {
        _db = db;
        _logger = logFactory.GetLogger<AliasSearchDbAction>();
        _macroManager = new(db, logFactory, converter);
    }

    #endregion Constructors

    #region Methods

    public IEnumerable<AliasQueryResult> Search(string name, long idSession)
    {
        using var _ = _logger.MeasureExecutionTime(this);
        var sql = @$"
                select
                    an.Name       as {nameof(AliasQueryResult.Name)},
                    a.Id          as {nameof(AliasQueryResult.Id)},
                    a.arguments   as {nameof(AliasQueryResult.Parameters)},
                    a.file_name   as {nameof(AliasQueryResult.FileName)},
                    a.notes       as {nameof(AliasQueryResult.Notes)},
                    a.run_as      as {nameof(AliasQueryResult.RunAs)},
                    a.start_mode  as {nameof(AliasQueryResult.StartMode)},
                    a.working_dir as {nameof(AliasQueryResult.WorkingDirectory)},
                    a.icon        as {nameof(AliasQueryResult.Icon)},
                    a.thumbnail   as {nameof(AliasQueryResult.Thumbnail)},
                    a.lua_script  as {nameof(AliasQueryResult.LuaScript)},
                    c.exec_count  as {nameof(AliasQueryResult.Count)},
                    s.synonyms    as {nameof(AliasQueryResult.Synonyms)},
                    s.Synonyms    as {nameof(AliasQueryResult.SynonymsWhenLoaded)},
                    a.exec_count  as {nameof(AliasQueryResult.Count)}
                from
                    alias a
                    left join alias_name            an on a.id         = an.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join data_alias_synonyms_v s on s.id_alias   = a.id
                where
                    a.id_session = @idSession
                    and an.Name like @name
                    and a.hidden = 0
                order by
                    c.exec_count desc,
                    an.name";

        name = $"{name ?? string.Empty}%";
        var results = _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, new { name, idSession }));

        results = _macroManager.UpgradeToComposite(results);
        return results ?? AliasQueryResult.NoResult;
    }

    public IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(string name, long idSession)
    {
        const string sql = @$"
                select
                    an.Name || ':' || aa.name         as {nameof(AliasQueryResult.Name)},
                    a.Id                              as {nameof(AliasQueryResult.Id)},
                    COALESCE(a.arguments, '') || ' ' || aa.argument as {nameof(AliasQueryResult.Parameters)},
                    a.file_name                       as {nameof(AliasQueryResult.FileName)},
                    a.notes                           as {nameof(AliasQueryResult.Notes)},
                    a.run_as                          as {nameof(AliasQueryResult.RunAs)},
                    a.start_mode                      as {nameof(AliasQueryResult.StartMode)},
                    a.working_dir                     as {nameof(AliasQueryResult.WorkingDirectory)},
                    a.icon                            as {nameof(AliasQueryResult.Icon)},
                    a.thumbnail                       as {nameof(AliasQueryResult.Thumbnail)},
                    a.lua_script                      as {nameof(AliasQueryResult.LuaScript)},
                    c.exec_count                      as {nameof(AliasQueryResult.Count)},
                    s.synonyms                        as {nameof(AliasQueryResult.Synonyms)},
                    s.Synonyms                        as {nameof(AliasQueryResult.SynonymsWhenLoaded)},
                    a.exec_count                      as {nameof(AliasQueryResult.Count)}
                from
                    alias a
                    left join alias_name            an on a.id         = an.id_alias
                    inner join alias_argument       aa on a.id         = aa.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join data_alias_synonyms_v s on s.id_alias   = a.id
                where
                    a.id_session = @idSession
                    and an.Name || ':' || aa.name  like @name
                    and a.hidden = 0
                order by
                    c.exec_count desc,
                    an.name";

        name = $"{name ?? string.Empty}%";
        var results = _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, new { name, idSession }));

        results = _macroManager.UpgradeToComposite(results);
        return results ?? AliasQueryResult.NoResult;
    }

    #endregion Methods
}