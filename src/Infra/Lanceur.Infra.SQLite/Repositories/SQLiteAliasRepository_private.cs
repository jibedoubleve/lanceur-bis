using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.SQLite.DbActions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.Repositories;

public partial class SQLiteAliasRepository
{
    #region Fields

    private readonly IDbActionFactory _dbActionFactory;
    private readonly GetAllAliasDbAction _getAllAliasDbAction;
    private readonly ILogger<SQLiteAliasRepository> _logger;

    [GeneratedRegex(@"(www?|http?|https?|ftp):\/\/[^\s/$.?#].[^\s]*$|^[a-zA-Z0-9-]+\.[a-zA-Z]{2,6}(\.[a-zA-Z]{2,})?$")]
    private static partial Regex BuildRegexSelectUrl();

    #endregion

    #region Methods

    private static IEnumerable<AdditionalParameter> GetAdditionalParameter(IDbConnection conn, IEnumerable<long> ids)
    {
        const string sql = """
                           select 
                           	id_alias as AliasId,
                           	id       as Id,
                           	name     as Name,
                           	argument as Parameter
                           from alias_argument	
                           where id_alias in @ids
                           """;
        return conn.Query<AdditionalParameter>(sql, new { ids });
    }

    private AliasQueryResult? GetById(IDbConnection conn, long id)
        => _dbActionFactory.AliasManagement.GetById(conn, id);

    private static void MergeHistory(IDbTransaction tx, IEnumerable<long> fromAliases, long toAlias)
    {
        const string sql = """
                           update alias_usage 
                           set id_alias = @destinationId
                           where id_alias in @sourceIds
                           """;
        tx.Connection!.Execute(sql, new { destinationId = toAlias, sourceIds = fromAliases });
    }

    private void Remove(IDbTransaction tx, IEnumerable<long> aliasIds)
    {
        var list = aliasIds.ToArray();
        _logger.LogInformation("Hard remove of {Count} alias(es) from database", list.Length);
        foreach (var item in list)
        {
            _dbActionFactory.AliasManagement.Remove(tx, item);
        }
    }

    private void Restore(IDbTransaction tx, AliasQueryResult alias)
    {
        const string sql = """
                           update alias 
                           set
                                deleted_at = null,
                                hidden     = 0
                           where id = @id
                           """;
        tx.Connection!.Execute(sql, new { id = alias.Id });
    }

    private void SaveOrUpdate(IDbTransaction tx, AliasQueryResult alias) =>
        _dbActionFactory.SaveManagement.SaveOrUpdate(tx, alias);

    private static string[] GetSynonyms(IDbConnection conn, IEnumerable<long> ids)
    {
        const string sql = "select name from alias_name where id_alias in @ids";
        return conn.Query<string>(sql, new { ids }).ToArray();
    }
    #endregion
}