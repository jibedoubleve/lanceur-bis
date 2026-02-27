using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Core.Models;

public abstract class SelfExecutableQueryResult : ExecutableQueryResult, ISelfExecutable
{
    protected SelfExecutableQueryResult()
    {           
        var type = GetType();
        
        Name = type.GetCustomAttribute<ReservedAliasAttribute>()?.Name
            ?? type.GetCustomAttribute<MacroAttribute>()?.Name;
        
        Description = type.GetCustomAttribute<DescriptionAttribute>()?.Description;
        
    }
    #region Properties

    public override string Icon => "AppGeneric24";

    #endregion

    #region Methods

    public abstract Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null);

    #endregion
}