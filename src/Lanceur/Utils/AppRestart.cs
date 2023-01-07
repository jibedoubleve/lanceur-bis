using Lanceur.SharedKernel.Utils;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Lanceur.Utils
{
    public interface IAppRestart
    {
        #region Methods

        void Restart();

        #endregion Methods
    }

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

    public class DummyAppRestart : IAppRestart
    {
        #region Methods

        public void Restart()
        { }

        #endregion Methods
    }
}