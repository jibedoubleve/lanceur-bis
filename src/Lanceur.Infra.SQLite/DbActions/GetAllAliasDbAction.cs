using Dapper;
using Lanceur.Core.Models;
using Lanceur.Core.Services;

namespace Lanceur.Infra.SQLite.DbActions;

public class GetAllAliasDbAction
{
    
    #region Fields

    private readonly IDbConnectionManager _db;
    private readonly IAppLogger _log;

    #endregion Fields
    
    #region Constructors

    public GetAllAliasDbAction(IDbConnectionManager db, IAppLoggerFactory logFactory)
    {
        _db  = db;
        _log = logFactory.GetLogger<AliasDbAction>();
    }

    #endregion Constructors
    
    #region Methods
    
    public IEnumerable<AliasQueryResult> GetAll(long? idSession = null)
    {
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
                    c.exec_count  as {nameof(AliasQueryResult.Count)},
                    s.synonyms    as {nameof(AliasQueryResult.Synonyms)},
                    s.Synonyms    as {nameof(AliasQueryResult.SynonymsPreviousState)}
                from
                    alias a
                    left join alias_name an            on a.id         = an.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join data_alias_synonyms_v s on s.id_alias   = a.id";
        if (idSession.HasValue)
        {
         sql += @"
                where
                  a.id_session = @idSession
                  and a.hidden = 0";
        }
        sql += @"
                order by
                  c.exec_count desc,
                  an.name";
        var parameters = new { idSession };

        var result =
            _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, parameters));
        return result ?? AliasQueryResult.NoResult;
    }
    
    public IEnumerable<AliasQueryResult> GetAllAliasWithAdditionalParameters(long idSession)
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
                    c.exec_count                      as {nameof(AliasQueryResult.Count)},
                    s.synonyms                        as {nameof(AliasQueryResult.Synonyms)},
                    s.Synonyms                        as {nameof(AliasQueryResult.SynonymsPreviousState)}
                from
                    alias a
                    left join alias_name an            on a.id         = an.id_alias
                    left join stat_execution_count_v c on c.id_keyword = a.id
                    inner join alias_argument       aa on a.id         = aa.id_alias
                    inner join data_alias_synonyms_v s on s.id_alias   = a.id
                where
                    a.id_session = @idSession
                    and a.hidden = 0
                order by
                    c.exec_count desc,
                    an.name";
        var parameters = new { idSession };

        var result =
            _db.WithinTransaction(tx => tx.Connection.Query<AliasQueryResult>(sql, parameters));
        return result ?? AliasQueryResult.NoResult;
    }
    
    #endregion Methods
}