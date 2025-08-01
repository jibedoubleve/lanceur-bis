using System.Globalization;
using Lanceur.SharedKernel;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Tests.Tools.SQL;

public class SqlGenerator : SqlGeneratorBase
{
    #region Fields

    private readonly long _idAlias;

    #endregion

    #region Constructors

    public SqlGenerator(long idAlias)
    {
        _idAlias = idAlias;
        const string sql = "insert into alias (id) values ({0});";
        Sql.AppendLine(sql.Format(idAlias));
    }

    #endregion

    #region Methods

    public SqlGenerator WithAdditionalParameter(params (string Name, string Argument)[] parameters)
    {
        foreach (var parameter in parameters)
        {
            const string sql = "insert into alias_argument (id_alias, name, argument) values ({0}, '{1}', '{2}');";
            Sql.AppendLine(
                sql.Format(
                    _idAlias, parameter.Name, parameter.Argument));
        }

        return this;
    }

    public SqlGenerator WithArguments(string arguments)
    {
        const string sql = "update alias set arguments = '{0}' where id = {1};";
        Sql.AppendLine(sql.Format(arguments, _idAlias));
        return this;
    }

    public SqlGenerator WithConfirmationRequiredActivated()
    {
        const string sql = "update alias set  confirmation_required = {1} where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, 1));
        return this;
    }

    public SqlGenerator WithCount(int count)
    {
        const string sql = "update alias set exec_count = {0} where id = {1};";
        Sql.AppendLine(sql.Format(count, _idAlias));
        return this;
    }

    public SqlGenerator WithDeletedAt(DateTime date)
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

    public SqlGenerator WithFileName(string fileName)
    {
        const string sql = "update alias set file_name = '{0}' where id = {1};";
        Sql.AppendLine(sql.Format(fileName, _idAlias));
        return this;
    }

    public SqlGenerator WithHiddenFlag()
    {
        const string sql = "update alias set  hidden = {1} where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, 1));
        return this;
    }

    public SqlGenerator WithIcon(string icon)
    {
        const string sql = "update alias set icon = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, icon));
        return this;
    }

    public SqlGenerator WithLuaScript(string script)
    {
        const string sql = "update alias set lua_script = '{0}' where id = {1};";
        Sql.AppendLine(sql.Format(script, _idAlias));
        return this;
    }

    public SqlGenerator WithNames(params string[] names)
    {
        const string sql = "insert into alias_name (name, id_alias) values ('{0}', {1});";

        foreach (var name in names) Sql.AppendLine(sql.Format(name, _idAlias));

        return this;
    }

    public SqlGenerator WithNotes(string notes)
    {
        const string sql = "update alias set notes = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, notes));
        return this;
    }

    public SqlGenerator WithRunAs(Constants.RunAs runAs)
    {
        const string sql = "update alias set run_as = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, runAs));
        return this;
    }

    public SqlGenerator WithStartMode(Constants.StartMode startMode)
    {
        const string sql = "update alias set start_mode = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, startMode));
        return this;
    }

    public SqlGenerator WithUsage(params DateTime[] usage)
    {
        foreach (var date in usage)
        {
            const string sql = "insert into alias_usage (id_alias, time_stamp) values ({0}, '{1}');";
            Sql.AppendLine(sql.Format(_idAlias, date.ToString("o", CultureInfo.InvariantCulture)));
        }

        return this;
    }

    public SqlGenerator WithWorkingDir(string workingDirectory)
    {
        const string sql = "update alias set working_dir = '{1}' where id = {0};";
        Sql.AppendLine(sql.Format(_idAlias, workingDirectory));
        return this;
    }

    #endregion
}