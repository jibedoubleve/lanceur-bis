using Lanceur.Core;
using System.Reflection;

namespace Lanceur.Infra.Utils;

public class MacroLister
{
    #region Fields

    private readonly object _source;
    private MacroAttribute[] _macros;

    #endregion Fields

    #region Constructors

    public MacroLister(object source)
    {
        _source = source;
    }

    #endregion Constructors

    #region Methods

    private MacroAttribute[] GetMacroAttributes()
    {
        return Assembly.GetAssembly(_source.GetType())!.GetTypes()
                       .SelectMany(t => t.GetCustomAttributes<MacroAttribute>())
                       .ToArray();
    }

    public IEnumerable<MacroAttribute> GetAttributes()
    {
        _macros ??= GetMacroAttributes();
        return _macros.ToArray();
    }

    public IEnumerable<MacroAttribute> GetVisibleAttributes()
    {
        _macros ??= GetMacroAttributes();

        return _macros.Where(t => t.IsVisible)
                      .ToArray(); ;
    }

    #endregion Methods
}