using System.Diagnostics;
using Lanceur.Core.Services;
using Lanceur.Infra.Mappers;

namespace Lanceur.Infra.Services;

public class ProcessLauncherWin32 : IProcessLauncher
{
    #region Methods

    /// <inheritdoc />
    public void Open(string path) => Process.Start("explorer.exe", path);

    /// <inheritdoc />
    public void Start(ProcessContext context)
    {
        Process.Start(
            context.ToProcessStartInfo()
        );
    }

    #endregion
}