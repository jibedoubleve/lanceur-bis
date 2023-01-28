using Lanceur.Core.Services;
using Lanceur.SharedKernel.Utils;
using Lanceur.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using Splat;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Lanceur
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructors

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            Bootstrapper.Initialise();
        }

        #endregion Constructors

        #region Methods

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var notify = Locator.Current.GetService<IUserNotification>();
            notify.Error(e.Exception.Message, e.Exception);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ToastNotificationManagerCompat.Uninstall();
            SingleInstance.ReleaseMutex();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.Current.SetTheme();

            if (!SingleInstance.WaitOne())
            {
                var notify = Locator.Current.GetService<IUserNotification>();
                notify.Warning("An instance of Lanceur is already running.");
                Environment.Exit(0);
            }
        }

        #endregion Methods
    }
}