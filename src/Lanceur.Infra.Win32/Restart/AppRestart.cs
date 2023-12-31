using System.Reflection;
using System.Windows;
using Lanceur.SharedKernel.Utils;

namespace Lanceur.Infra.Win32.Restart
{
    public class AppRestart : IAppRestart
    {
        #region Fields

        public readonly Mutex _mutex = SingleInstance.Mutex;

        #endregion Fields

        #region Methods

        public void Restart()
        {
            _mutex.ReleaseMutex();

            var process = Assembly.GetEntryAssembly().Location.Replace("dll", "exe");
            System.Diagnostics.Process.Start(process);
            Application.Current.Shutdown();
        }

        #endregion Methods
    }
}