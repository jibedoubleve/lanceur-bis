﻿using Dapper;
using Lanceur.Core.Models;
using Lanceur.Infra.Logging;
using Lanceur.Infra.SQLite.DataAccess;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.SQLite.DbActions
{
    public class SetUsageDbAction
    {
        #region Fields

        private readonly AliasDbAction _aliasDbAction;
        private readonly IDbConnectionManager _db;
        private readonly ILogger<SetUsageDbAction> _logger;

        #endregion Fields

        #region Constructors

        public SetUsageDbAction(IDbConnectionManager db, ILoggerFactory logFactory)
        {
            _db = db;
            _logger = logFactory.GetLogger<SetUsageDbAction>();

            _aliasDbAction = new(db, logFactory);
        }

        #endregion Constructors

        #region Methods

        public void SetUsage(ref QueryResult alias, long idSession)
        {
            ArgumentNullException.ThrowIfNull(alias);
            
            if ((alias?.Id ?? 0) == 0)
            {
                if (_aliasDbAction.GetExact(alias?.Name) is { } a)
                {
                    alias!.Id = a.Id;
                }
                else { _aliasDbAction.CreateInvisible(ref alias); }
            }
            
            AddHistory(ref alias, idSession);
            IncrementCounter(alias);
        }

        private void IncrementCounter(QueryResult alias)
        {
            alias.Count++;
            const string sql = @"
                update alias 
                set 
                    exec_count = @counter 
                where 
                    id = @id";
            var param = new { id = alias.Id, counter = alias.Count };
            _db.WithinTransaction(tx => tx.Connection.Execute(sql, param));
        }

        private void AddHistory(ref QueryResult alias, long idSession)
        {
            const string sql = @"
                    insert into alias_usage (
                        id_alias,
                        id_session,
                        time_stamp

                    ) values (
                        @idAlias,
                        @idSession,
                        @now
                    )";

            var param = new { idAlias = alias.Id, idSession, now = DateTime.Now };
            _db.WithinTransaction(tx => tx.Connection.Execute(sql, param));
        }

        #endregion Methods
    }
}