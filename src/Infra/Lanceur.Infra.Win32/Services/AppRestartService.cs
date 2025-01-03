using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Infra.Win32.Services;

public class AppRestartService : IAppRestartService
{
    #region Fields

    private readonly Mutex _mutex = SingleInstance.Mutex;

    #endregion

    #region Methods

    public void Restart()
    {
        _mutex.ReleaseMutex();

        var process = Assembly.GetEntryAssembly()!.Location.Replace("dll", "exe");
        Process.Start(process);
        Application.Current.Shutdown();
    }

    #endregion
}