using Lanceur.Core.Scripting;

namespace Lanceur.Tests.Tools.Extensions;

public static class ScriptResultExtensions
{
    #region Methods

    public static bool IsOnError(this ScriptResult scriptResult) => scriptResult.Exception is not null;

    #endregion
}