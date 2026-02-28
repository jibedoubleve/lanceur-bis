using System.Globalization;
using System.Text;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Tests.Tools.SQL;

public class SqlAliasBuilder : SqlBuilderBase
{
    #region Fields

    private readonly long _idAlias;

    #endregion

    #region Constructors

    internal SqlAliasBuilder(long idAlias, StringBuilder builder)
    {
        _idAlias = idAlias;
        Sql = builder;
        AppendAlias();
    }

    private  SqlAliasBuilder() { }

    #endregion

    #region Properties

    private static string RandomString => Guid.NewGuid().ToString();

    #endregion

    #region Methods

    private void AppendAlias()
    {
        const string sql = "insert into alias (id, file_name) values ({0}, '{1}');";
        Sql.AppendLine("-----------------------------------------------------------");
        Sql.AppendLine(sql.Format(_idAlias, RandomString));
    }

    public SqlAliasBuilder WithAdditionalParameters(params (string Name, string Argument)[] parameters)
    {
        if (parameters.Length == 0) { parameters = [($"{RandomString}", $"{RandomString}")]; }

        foreach (var parameter in parameters)
        {
            const string sql = "insert into alias_argument (id_alias, name, argument) values ({0}, '{1}', '{2}');";
            Sql.AppendLine(
                sql.Format(
                    _idAlias,
                    parameter.Name,
                    parameter.Argument
                )
            );
        }

        return this;
    }

    public SqlAliasBuilder WithArguments(string arguments)
    {
        const string sql = "update alias set arguments = '{0}' where id = {1};";
        Sql.AppendLine(sql.Format(arguments, _idAlias));
        return this;
    }

    public SqlAliasBuilder WithCount(int count)
    {
        const string sql = "update alias set exec_count = {0} where id = {1};";
        Sql.AppendLine(sql.Format(count, _idAlias));
        return this;
    }

    public SqlAliasBuilder WithDeletedAt(DateTime date)
    {
        const string sql = "update alias set deleted_at = '{0}' where id = {1};";
        Sql.AppendLine(
            sql.Format(
                date.ToString("o", CultureInfo.InvariantCulture),
                _idAlias
            )
        );
        return this;
    }

    public SqlAliasBuilder WithFileName(string fileName)
    {
        const string sql = "update alias set file_name = '{0}' where id = {1};";
        Sql.AppendLine(sql.Format(fileName, _idAlias));
        return this;
    }

    public SqlAliasBuilder WithHiddenFlag()
    {
        const string sql = "update alias set  hidden = {1} where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, 1));
        return this;
    }

    public SqlAliasBuilder WithIcon(string icon)
    {
        const string sql = "update alias set icon = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, icon));
        return this;
    }

    public SqlAliasBuilder WithLuaScript(string? script)
    {
        script = script is null ? "null" : $"'{script}'";
        const string sql = "update alias set lua_script = {0} where id = {1};";
        Sql.AppendLine(sql.Format(script, _idAlias));
        return this;
    }

    public SqlAliasBuilder WithNotes(string notes)
    {
        const string sql = "update alias set notes = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, notes));
        return this;
    }

    public SqlAliasBuilder WithRandomFileName()
    {
        WithFileName(Guid.NewGuid().ToString());
        return this;
    }

    public SqlAliasBuilder WithRunAs(Constants.RunAs runAs)
    {
        const string sql = "update alias set run_as = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, runAs));
        return this;
    }

    public SqlAliasBuilder WithStartMode(Constants.StartMode startMode)
    {
        const string sql = "update alias set start_mode = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, startMode));
        return this;
    }

    public SqlAliasBuilder WithSynonyms(params string[] names)
    {
        const string sql = "insert into alias_name (name, id_alias) values ('{0}', {1});";

        if (names.Length == 0)
        {
            Sql.AppendLine(sql.Format(RandomString, _idAlias));
            return this;
        }

        foreach (var name in names) Sql.AppendLine(sql.Format(name, _idAlias));

        return this;
    }

    public SqlAliasBuilder WithThumbnail(string thumbnail)
    {
        const string sql = "update alias set thumbnail = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, thumbnail));
        return this;
    }

    public SqlAliasBuilder WithUsage(params string[] dateString)
    {
        try
        {
            var dates = dateString.Select(DateTime.Parse).ToArray();
            return WithUsage(dates);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException(
                "Impossible to cast a string into date when creating an alias execution date.",
                ex
            );
        }
    }

    public SqlAliasBuilder WithUsage(params DateTime[] usage)
    {
        foreach (var date in usage)
        {
            const string sql = "insert into alias_usage (id_alias, time_stamp) values ({0}, '{1}');";
            Sql.AppendLine(sql.Format(_idAlias, date.ToString("o", CultureInfo.InvariantCulture)));
        }

        return this;
    }

    public SqlAliasBuilder WithWorkingDir(string workingDirectory)
    {
        const string sql = "update alias set working_dir = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, workingDirectory));
        return this;
    }

    #endregion
}