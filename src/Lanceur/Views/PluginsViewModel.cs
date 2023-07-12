using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Ui;
using Lanceur.Views.Helpers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace Lanceur.Views
{
    public class PluginsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly IPluginConfigRepository _pluginConfigRepository;

        private readonly ISchedulerProvider _schedulers;

        #endregion Fields

        #region Constructors

        public PluginsViewModel(
            ISchedulerProvider schedulers = null,
            IUserNotification notify = null,
            IPluginConfigRepository pluginConfigRepository = null)
        {
            var l = Locator.Current;

            notify ??= l.GetService<IUserNotification>();

            _schedulers = schedulers ?? l.GetService<ISchedulerProvider>();
            _pluginConfigRepository = pluginConfigRepository ?? l.GetService<IPluginConfigRepository>();

            Activate = ReactiveCommand.Create(OnActivate, outputScheduler: _schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .Subscribe(response =>
                {
                    PluginConfigurations = new(response.PluginConfigurations);
                });
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ActivationContext> Activate { get; private set; }
        [Reactive] public ObservableCollection<PluginConfigurationViewModel> PluginConfigurations { get; set; }

        #endregion Properties

        #region Methods

        private ActivationContext OnActivate()
        {
            var pluginConfigurations = _pluginConfigRepository
                .GetPluginConfigurations()
                .ToViewModel();

            var context = new ActivationContext()
            {
                PluginConfigurations = pluginConfigurations
            };

            return context;
        }

        #endregion Methods

        #region Classes

        public class ActivationContext
        {
            #region Properties

            public IEnumerable<PluginConfigurationViewModel> PluginConfigurations { get; internal set; }

            #endregion Properties
        }

        #endregion Classes
    }
}