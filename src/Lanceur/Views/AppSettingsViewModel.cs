using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.Infra.Managers;
using Lanceur.Infra.Win32.Restart;
using Lanceur.Schedulers;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Humanizer;
using Lanceur.Infra.Logging;
using Lanceur.Utils;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lanceur.Views
{
    public class AppSettingsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly Interaction<Unit, string> _askFile;
        private readonly IDelay _delay;
        private readonly ILogger<AppSettingsView> _logger;
        private readonly LoggingLevelSwitch _logLevelSwitch;
        private readonly INotification _notification;
        private readonly IAppRestart _restart;
        private readonly IDbRepository _service;
        private readonly ISettingsFacade _settingsFacade;

        #endregion Fields

        #region Constructors

        public AppSettingsViewModel(
            ISchedulerProvider schedulers = null,
            IUserNotification notify = null,
            ISettingsFacade settingsFacade = null,
            IDbRepository dataService = null,
            IDelay delay = null,
            IAppRestart restart = null,
            INotification notification = null,
            ILoggerFactory factory = null,
            LoggingLevelSwitch logLevelSwitch = null
        )
        {
            var l = Locator.Current;
            schedulers ??= l.GetService<ISchedulerProvider>();
            factory ??= l.GetService<ILoggerFactory>();
            _logger = factory.GetLogger<AppSettingsView>();
            _askFile = Interactions.SelectFile(schedulers.MainThreadScheduler);
            _logLevelSwitch = logLevelSwitch ?? l.GetService<LoggingLevelSwitch>();
            _service = dataService ?? l.GetService<IDbRepository>();
            notify ??= l.GetService<IUserNotification>();
            _delay = delay ?? l.GetService<IDelay>();
            _restart = restart ?? l.GetService<IAppRestart>();
            _notification = notification ?? l.GetService<INotification>();
            _settingsFacade = settingsFacade ?? l.GetService<ISettingsFacade>();

            ChangeLogVerbosity =
                ReactiveCommand.Create<LogLevel>(OnChangeLogVerbosity, outputScheduler: schedulers.MainThreadScheduler);
            ChangeLogVerbosity.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveSettings = ReactiveCommand.Create(OnSaveSettings, outputScheduler: schedulers.MainThreadScheduler);
            SaveSettings.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectDatabase = ReactiveCommand.CreateFromTask(async () => await AskFile.Handle(Unit.Default),
                                                            outputScheduler: schedulers.MainThreadScheduler);
            SelectDatabase.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(schedulers.MainThreadScheduler)
                .Subscribe(response =>
                {
                    DbPath = response.DbPath;
                    HotKeySection = response.AppSettings.HotKey;
                    RestartDelay = response.AppSettings.RestartDelay;
                    Sessions = new(response.Sessions);
                    CurrentSession = Sessions.SingleOrDefault(s => s.Id == response.AppSettings.IdSession);
                    SettingsMemento = response.SettingsMemento;
                });

            this.WhenAnyObservable(vm => vm.SelectDatabase)
                .ObserveOn(schedulers.MainThreadScheduler)
                .BindTo(this, vm => vm.DbPath);
        }

        #endregion Constructors

        #region Properties

        private SettingsMementoManager SettingsMemento { get; set; }

        public ReactiveCommand<Unit, ActivationContext> Activate { get; }

        public Interaction<Unit, string> AskFile => _askFile;

        public ReactiveCommand<LogLevel, Unit> ChangeLogVerbosity { get; }

        [Reactive] public Session CurrentSession { get; set; }

        [Reactive] public string DbPath { get; set; }

        [Reactive] public HotKeySection HotKeySection { get; set; }

        [Reactive] public double RestartDelay { get; set; }

        public ReactiveCommand<Unit, Unit> SaveSettings { get; }

        public ReactiveCommand<Unit, string> SelectDatabase { get; }

        [Reactive] public ObservableCollection<Session> Sessions { get; set; }

        [Reactive] public bool ShowResult { get; set; }

        #endregion Properties

        #region Methods

        private TimeSpan GetDelay()
        {
            var delay = _settingsFacade.Application.RestartDelay;
            var time = delay.Milliseconds();
            return time;
        }

        private ActivationContext OnActivate() => new()
        {
            AppSettings = _settingsFacade.Application,
            DbPath = _settingsFacade.Local.DbPath,
            Sessions = _service.GetSessions(),
            SettingsMemento = SettingsMementoManager.GetInitialState(_settingsFacade)
        };

        private void OnChangeLogVerbosity(LogLevel level)
        {
            _logger.LogInformation("Set logging level threshold to {Level}", level);
            _logLevelSwitch.MinimumLevel = level.ToSerilogLogLevel();
        }

        private async void OnSaveSettings()
        {
            //Save DB Path in property file
            _settingsFacade.Local.DbPath = DbPath?.Replace("\"", "");

            // Save hotkey & Session in DB
            _settingsFacade.Application.Window.ShowResult = ShowResult;
            _settingsFacade.Application.RestartDelay = RestartDelay;
            _settingsFacade.Application.HotKey = HotKeySection;
            _settingsFacade.Application.Window.ShowAtStartup = ShowResult;
            if (CurrentSession is not null) _settingsFacade.Application.IdSession = CurrentSession.Id;

            //Save settings
            _settingsFacade.Save();

            if (SettingsMemento.HasStateChanged(_settingsFacade))
            {
                var time = GetDelay();

                _notification.Information(
                    $"Application settings saved. Restart in {time.TotalMilliseconds} milliseconds");
                await _delay.Of(time);
                _restart.Restart();
            }
            else
            {
                _notification.Information("Saved configuration.");
            }
        }

        #endregion Methods

        #region Classes

        public class ActivationContext
        {
            #region Properties

            public AppConfig AppSettings { get; internal set; }
            public string DbPath { get; internal set; }
            public IEnumerable<Session> Sessions { get; set; }
            public SettingsMementoManager SettingsMemento { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}