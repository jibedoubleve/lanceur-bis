using System.Reflection;

namespace Lanceur.Core;

public class AssemblySource
{
    #region Properties

    public Assembly MacroSource { get;  init; }
    public Assembly ReservedKeywordSource { get;  init; }

    #endregion
}