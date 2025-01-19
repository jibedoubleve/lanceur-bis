using System.Data;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DataAccess;

namespace Lanceur.Infra.SQLite.DbActions;

internal class GetAllAliasDbAction
{
    #region Fields

    private readonly IDbConnectionManager _db;

    #endregion

    #region Constructors

    internal GetAllAliasDbAction(IDbConnectionManager db) => _db = db;

    #endregion

    #region Methods

    internal IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters(IDbTransaction tx)
    {
        const string sql = $"""
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
                                a.exec_count                      as {nameof(AliasQueryResult.Count)},
                                s.synonyms                        as {nameof(AliasQueryResult.Synonyms)},
                                s.Synonyms                        as {nameof(AliasQueryResult.SynonymsWhenLoaded)}
                            from
                                alias a
                                left join alias_name an            on a.id       = an.id_alias                    
                                inner join alias_argument       aa on a.id       = aa.id_alias
                                inner join data_alias_synonyms_v s on s.id_alias = a.id
                            where a.hidden = 0
                            order by
                                a.exec_count desc,
                                an.name
                            """;

        return tx.Connection!.Query<AliasQueryResult>(sql);
    }

    #endregion
}