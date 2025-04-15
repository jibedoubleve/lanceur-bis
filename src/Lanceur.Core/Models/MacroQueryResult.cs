using System.ComponentModel;

namespace Lanceur.Core.Models;

public abstract class MacroQueryResult : SelfExecutableQueryResult
{
    #region Constructors

    protected MacroQueryResult() => Description = GetDescription();

    #endregion

    #region Methods

    private string GetDescription()
    {
        var attribute = GetType().GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
        if (attribute is DescriptionAttribute description) return description.Description;

        return string.Empty;
    }

    public abstract SelfExecutableQueryResult Clone();

    #endregion
}