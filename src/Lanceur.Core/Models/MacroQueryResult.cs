using System.ComponentModel;
using System.Reflection;

namespace Lanceur.Core.Models;

public abstract class MacroQueryResult : SelfExecutableQueryResult
{
    #region Constructors

    protected MacroQueryResult()
    {
        Name = GetName();
        Description = GetDescription();
    }

    #endregion

    #region Methods

    private string GetDescription()
    {
        var attribute = GetType().GetCustomAttribute<DescriptionAttribute>();
        return attribute is not null
            ? attribute.Description
            : string.Empty;
    }

    private string GetName()
    {
        var attribute = GetType().GetCustomAttribute<MacroAttribute>();

        return attribute is not null
            ? attribute.Name.ToUpper().Replace("@", string.Empty)
            : string.Empty;
    }

    public abstract SelfExecutableQueryResult Clone();

    #endregion
}