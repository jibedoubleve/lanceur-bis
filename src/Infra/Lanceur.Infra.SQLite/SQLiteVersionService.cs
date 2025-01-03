﻿using Dapper;
using Lanceur.Core.Services;
using Lanceur.Infra.SQLite.DataAccess;
using Lanceur.Infra.SQLite.Repositories;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Infra.SQLite;

public class SQLiteVersionService : SQLiteRepositoryBase, IDataStoreVersionService
{
    #region Constructors

    public SQLiteVersionService(IDbConnectionManager manager) : base(manager) { }

    #endregion Constructors

    #region Methods

    public Version GetCurrentDbVersion()
    {
        var version = Db.WithinTransaction(tx => tx.Connection.Query<string>(SQL.GetDbVersion).FirstOrDefault());

        return version.IsNullOrEmpty()
            ? new()
            : new Version(version);
    }

    public bool IsUpToDate(Version goalVersion)
    {
        var version = Db.WithinTransaction(tx => tx.Connection.Query<string>(SQL.GetDbVersion).FirstOrDefault());

        var currentVersion = version.IsNullOrEmpty()
            ? new()
            : new Version(version);

        return goalVersion <= currentVersion;
    }

    public bool IsUpToDate(string expectedVersion) => IsUpToDate(new Version(expectedVersion));

    public void SetCurrentDbVersion(Version version)
    {
        Db.WithinTransaction(
            tx =>
            {
                var exists = tx.Connection.ExecuteScalar<int>(SQL.DbVersionCount) >= 1;
                tx.Connection.Execute(exists ? SQL.UpdateDbVersion.Format(version) : SQL.SetDbVersion.Format(version));
            }
        );
    }

    #endregion Methods

    #region Classes

    private class SQL
    {
        #region Fields

        public const string DbVersionCount = @"select count(*) from settings where lower(s_key) = 'db_version'";
        public const string GetDbVersion = @"select s_value from settings where lower(s_key) = 'db_version'";
        public const string SetDbVersion = @"insert into settings (s_key, s_value) values ('db_version','{0}');";
        public const string UpdateDbVersion = "update settings set s_value = '{0}' where lower(s_key) = 'db_version'";

        #endregion Fields
    }

    #endregion Classes
}