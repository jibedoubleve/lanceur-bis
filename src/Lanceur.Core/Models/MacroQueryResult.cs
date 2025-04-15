using System.ComponentModel;

namespace Lanceur.Core.Models;

public abstract class MacroQueryResult : SelfExecutableQueryResult
{
    #region Constructors

    protected MacroQueryResult(Type type) => Description = GetDescription(type);

    #endregion

    #region Methods

    private string GetDescription(Type sourceType)
    {
        var attribute = sourceType.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
        if (attribute is DescriptionAttribute description) return description.Description;

        return string.Empty;
    }

    public abstract SelfExecutableQueryResult Clone();

    #endregion
}