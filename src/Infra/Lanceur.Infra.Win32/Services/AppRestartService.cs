using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Lanceur.Infra.Win32.Services;

public class AppRestartService : IAppRestartService
{
    #region Fields

    private readonly Mutex _mutex = new(true, @"Global\Lanceur2");

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Restart()
    {
        _mutex.ReleaseMutex();

        var process = Assembly.GetEntryAssembly()!.Location.Replace("dll", "exe");
        Process.Start(process);
        Application.Current.Shutdown();
    }

    #endregion
}