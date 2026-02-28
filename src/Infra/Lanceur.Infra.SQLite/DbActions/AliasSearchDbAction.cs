using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions;

public class AliasSearchDbAction
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;
    private readonly ILogger<AliasSearchDbAction> _logger;

    #endregion

    #region Constructors

    internal AliasSearchDbAction(ILoggerFactory logFactory, IDbActionFactory dbActionFactory)
    {
        _dbActionFactory = dbActionFactory;
        _logger = logFactory.GetLogger<AliasSearchDbAction>();
    }

    #endregion

    #region Methods

    internal IEnumerable<AliasQueryResult> Search(
        IDbConnection connection,
        string name = null,
        bool isReturnAllIfEmpty = false
    )
    {
        using var _ = _logger.WarnIfSlow(this);

        if (name.IsNullOrEmpty() && !isReturnAllIfEmpty) { return Array.Empty<AliasQueryResult>(); }

        var sql = $"""
                   select
                       an.Name                 as {nameof(AliasQueryResult.Name)},
                       a.Id                    as {nameof(AliasQueryResult.Id)},
                       a.arguments             as {nameof(AliasQueryResult.Parameters)},
                       a.file_name             as {nameof(AliasQueryResult.FileName)},
                       a.notes                 as {nameof(AliasQueryResult.Description)},
                       a.run_as                as {nameof(AliasQueryResult.RunAs)},
                       a.start_mode            as {nameof(AliasQueryResult.StartMode)},
                       a.working_dir           as {nameof(AliasQueryResult.WorkingDirectory)},
                       a.icon                  as {nameof(AliasQueryResult.Icon)},
                       a.thumbnail             as {nameof(AliasQueryResult.Thumbnail)},
                       a.lua_script            as {nameof(AliasQueryResult.LuaScript)},
                       a.exec_count            as {nameof(AliasQueryResult.Count)},
                       s.synonyms              as {nameof(AliasQueryResult.Synonyms)},
                       s.synonyms              as {nameof(AliasQueryResult.SynonymsWhenLoaded)},
                       a.confirmation_required as {nameof(AliasQueryResult.IsExecutionConfirmationRequired)},
                       a.hidden                as {nameof(AliasQueryResult.IsHidden)}
                   from
                       alias a
                       left join alias_name             an on a.id       = an.id_alias                    
                       left join data_alias_synonyms_v  s on s.id_alias = a.id
                   where
                       
                   """;
        if (!name.IsNullOrEmpty())
        {
            sql += """
                      an.Name like @name
                      and 
                   """;
        }

        sql += """
               a.deleted_at is null
                   and a.hidden = 0
               order by
                 a.exec_count desc,
                 an.name
               """;

        name = $"{name ?? string.Empty}%";
        var results = connection.Query<AliasQueryResult>(sql, new { name });

        results = _dbActionFactory.MacroManagement.UpgradeToComposite(connection, results);
        return results ?? AliasQueryResult.NoResult;
    }

    internal IEnumerable<AliasQueryResult> SearchAliasWithAdditionalParameters(IDbConnection connection, string name)
    {
        const string sql = $"""
                            select
                                an.Name || ':' || aa.name         as {nameof(AliasQueryResult.Name)},
                                a.Id                              as {nameof(AliasQueryResult.Id)},
                                COALESCE(a.arguments, '') || ' ' || aa.argument as {nameof(AliasQueryResult.Parameters)},
                                a.file_name                       as {nameof(AliasQueryResult.FileName)},
                                a.notes                           as {nameof(AliasQueryResult.Description)},
                                a.run_as                          as {nameof(AliasQueryResult.RunAs)},
                                a.start_mode                      as {nameof(AliasQueryResult.StartMode)},
                                a.working_dir                     as {nameof(AliasQueryResult.WorkingDirectory)},
                                a.icon                            as {nameof(AliasQueryResult.Icon)},
                                a.lua_script                      as {nameof(AliasQueryResult.LuaScript)},
                                a.exec_count                      as {nameof(AliasQueryResult.Count)},
                                s.synonyms                        as {nameof(AliasQueryResult.Synonyms)},
                                s.Synonyms                        as {nameof(AliasQueryResult.SynonymsWhenLoaded)},
                                a.exec_count                      as {nameof(AliasQueryResult.Count)}
                            from
                                alias a
                                left join alias_name            an on a.id       = an.id_alias
                                inner join alias_argument       aa on a.id       = aa.id_alias                    
                                inner join data_alias_synonyms_v s on s.id_alias = a.id
                            where
                                an.Name || ':' || aa.name  like @name
                                and a.hidden = 0
                                and a.deleted_at is null
                            order by
                                a.exec_count desc,
                                an.name
                            """;

        name = $"{name ?? string.Empty}%";
        var results = connection.Query<AliasQueryResult>(sql, new { name });

        results = _dbActionFactory.MacroManagement
                                  .UpgradeToComposite(connection, results);
        return results ?? AliasQueryResult.NoResult;
    }

    #endregion
}