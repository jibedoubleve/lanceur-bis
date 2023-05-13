using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Ui;
using Lanceur.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Lanceur.Views
{
    public class AppSettingsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly Interaction<Unit, string> _askFile;
        private readonly IDelay _delay;
        private readonly INotification _nofification;
        private readonly IAppRestart _restart;
        private readonly ISchedulerProvider _schedulers;
        private readonly IDataService _service;
        private readonly IAppSettingsService _settings;
        private readonly ISettingsService _stg;

        #endregion Fields

        #region Constructors

        public AppSettingsViewModel(
            ISchedulerProvider schedulers = null,
            IAppSettingsService settings = null,
            IUserNotification notify = null,
            ISettingsService stg = null,
            IDataService service = null,
            IDelay delay = null,
            IAppRestart restart = null,
            INotification nofification = null
            )
        {
            var l = Locator.Current;
            _schedulers = schedulers ?? l.GetService<ISchedulerProvider>();
            _settings = settings ?? l.GetService<IAppSettingsService>();

            _askFile = Interactions.SelectFile(_schedulers.MainThreadScheduler);
            _stg = stg ?? l.GetService<ISettingsService>();
            _service = service ?? l.GetService<IDataService>();
            notify ??= l.GetService<IUserNotification>();
            _delay = delay ?? l.GetService<IDelay>();
            _restart = restart ?? l.GetService<IAppRestart>();
            _nofification = nofification ?? l.GetService<INotification>();

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: _schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SaveSettings = ReactiveCommand.Create(OnSaveSettings, outputScheduler: _schedulers.MainThreadScheduler);
            SaveSettings.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            SelectDatabase = ReactiveCommand.CreateFromTask(async () => await AskFile.Handle(Unit.Default), outputScheduler: _schedulers.MainThreadScheduler);
            SelectDatabase.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .Subscribe(ctx =>
                {
                    DbPath = ctx.DbPath;
                    HotKeySection = ctx.AppSettings.HotKey;
                    RestartDelay = ctx.AppSettings.RestartDelay;
                    Sessions = new ObservableCollection<Session>(ctx.Sessions);
                    CurrentSession = (from s in Sessions
                                      where s.Id == ctx.AppSettings.IdSession
                                      select s).SingleOrDefault();
                    Context = ctx;
                });

            this.WhenAnyObservable(vm => vm.SelectDatabase)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .BindTo(this, vm => vm.DbPath);
        }

        #endregion Constructors

        #region Properties

        [Reactive] private ActivationContext Context { get; set; }
        public ReactiveCommand<Unit, ActivationContext> Activate { get; }
        public Interaction<Unit, string> AskFile => _askFile;
        [Reactive] public Session CurrentSession { get; set; }
        [Reactive] public string DbPath { get; set; }
        [Reactive] public HotKeySection HotKeySection { get; set; }
        [Reactive] public double RestartDelay { get; set; }
        public ReactiveCommand<Unit, Unit> SaveSettings { get; }
        public ReactiveCommand<Unit, string> SelectDatabase { get; }
        [Reactive] public ObservableCollection<Session> Sessions { get; set; }

        #endregion Properties

        #region Methods

        private TimeSpan GetDelay()
        {
            var delay = Context.AppSettings.RestartDelay;
            var time = TimeSpan.FromMilliseconds(delay);
            return time;
        }

        private ActivationContext OnActivate()
        {
            var context = new ActivationContext()
            {
                AppSettings = _settings.Load(),
                DbPath = _stg[Setting.DbPath],
                Sessions = _service.GetSessions()
            };

            return context;
        }

        private async void OnSaveSettings()
        {
            //Save DB Path in property file
            _stg[Setting.DbPath] = DbPath?.Replace("\"", "");
            _stg.Save();

            // Save hotkey & Session in DB
            Context.AppSettings.RestartDelay = RestartDelay;
            Context.AppSettings.HotKey = HotKeySection;
            if (CurrentSession is not null) { Context.AppSettings.IdSession = CurrentSession.Id; }

            //Save settings
            _settings.Save(Context.AppSettings);

            TimeSpan time = GetDelay();

            _nofification.Information($"Application settings saved. Restart in {time.TotalMilliseconds} milliseconds");
            await _delay.Of(time);
            _restart.Restart();
        }

        #endregion Methods

        #region Classes

        public class ActivationContext
        {
            #region Properties

            public AppSettings AppSettings { get; internal set; }
            public string DbPath { get; internal set; }
            public IEnumerable<Session> Sessions { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}