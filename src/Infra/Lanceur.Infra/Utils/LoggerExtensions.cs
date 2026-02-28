using Lanceur.Core.Services;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Utils;

public static class LoggerExtensions
{
    #region Methods

    public static IDisposable ScopeProcessStartInfo(this ILogger logger, ProcessContext psi)
        => logger.BeginSingleScope(
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

    #endregion
}