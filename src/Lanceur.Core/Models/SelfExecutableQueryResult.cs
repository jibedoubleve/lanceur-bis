using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Core.Models;

public abstract class SelfExecutableQueryResult : ExecutableQueryResult, ISelfExecutable
{
    #region Constructors

    protected SelfExecutableQueryResult()
    {
        var type = GetType();

        Name = type.GetCustomAttribute<ReservedAliasAttribute>()?.Name ??
               type.GetCustomAttribute<MacroAttribute>()?.Name
               ?? string.Empty;

        Description = type.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
    }

    #endregion

    #region Properties

    public override string Icon => "AppGeneric24";

    #endregion

    #region Methods

    public abstract Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline? cmdline = null);

    #endregion
}