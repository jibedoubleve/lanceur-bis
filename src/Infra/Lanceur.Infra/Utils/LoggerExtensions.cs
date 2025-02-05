using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Lanceur.SharedKernel.Logging;

namespace Lanceur.Infra.Utils;

public static class LoggerExtensions
{
    #region Methods

    public static IDisposable ScopeProcessStartInfo(this ILogger logger, ProcessStartInfo psi) => logger.BeginSingleScope(
        "ProcessStartInfo",
        new
        {
            psi.FileName,
            psi.Verb,
            psi.Arguments,
            psi.UseShellExecute,
            psi.WorkingDirectory,
            psi.WindowStyle
        }
    );

    #endregion Methods
}