using Lanceur.Core.Models;
using Lanceur.Core.Plugins;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui;
using Lanceur.Utils;
using Lanceur.Views.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class PluginsViewModel : RoutableViewModel
    {
        #region Fields

        private readonly Interaction<Unit, string> _askFile;
        private readonly INotification _notification;
        private readonly IPluginConfigRepository _pluginConfigRepository;
        private readonly IPluginInstaller _pluginInstaller;
        private readonly IAppRestart _restart;
        private readonly ISchedulerProvider _schedulers;
        private readonly IPluginUninstaller _uninstaller;

        #endregion Fields

        #region Constructors

        public PluginsViewModel(
            ISchedulerProvider schedulers = null,
            IUserNotification notify = null,
            IPluginUninstaller uninstaller = null,
            IPluginConfigRepository pluginConfigRepository = null,
            INotification notification = null,
            IAppRestart restart = null,
            IPluginInstaller pluginInstaller = null)
        {
            var l = Locator.Current;

            notify ??= l.GetService<IUserNotification>();

            _schedulers = schedulers ?? l.GetService<ISchedulerProvider>();
            _uninstaller = uninstaller ?? l.GetService<IPluginUninstaller>();
            _pluginConfigRepository = pluginConfigRepository ?? l.GetService<IPluginConfigRepository>();
            _notification = notification ?? l.GetService<INotification>();
            _restart = restart ?? l.GetService<IAppRestart>();
            _pluginInstaller = pluginInstaller ?? l.GetService<IPluginInstaller>();
            _askFile = Interactions.SelectFile(_schedulers.MainThreadScheduler);

            Activate = ReactiveCommand.CreateFromTask(OnActivateAsync, outputScheduler: _schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            InstallPlugin = ReactiveCommand.CreateFromTask(OnInstallPlugin, outputScheduler: _schedulers.MainThreadScheduler);
            InstallPlugin.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            Restart = ReactiveCommand.Create(OnRestart, outputScheduler: _schedulers.MainThreadScheduler);
            Restart.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .Subscribe(response =>
                {
                    PluginConfigurations = new(response.PluginConfigurations);
                });

            this.WhenAnyObservable(vm => vm.InstallPlugin)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .Subscribe(response =>
                {
                    PluginConfigurations.Add(response.ToViewModel());
                });
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ActivationContext> Activate { get; private set; }
        public Interaction<Unit, string> AskFile => _askFile;
        public ReactiveCommand<Unit, IPluginConfiguration> InstallPlugin { get; set; }
        [Reactive] public ObservableCollection<PluginConfigurationViewModel> PluginConfigurations { get; set; }

        public ReactiveCommand<Unit, Unit> Restart { get; }

        #endregion Properties

        #region Methods

        private async Task<ActivationContext> OnActivateAsync()
        {
            // Get all installed plugins
            var allPluginConfigurations = _pluginConfigRepository
                .GetPluginConfigurations()
                .ToViewModel();

            // Get all candidates for uninstall
            // and remove them from the list
            var candidates = await _uninstaller.GetCandidatesAsync();

            var pluginConfigurations = (from p in allPluginConfigurations
                                        where !(from c in candidates
                                                select c.Directory
                                        ).Contains(p.Dll.GetDirectoryName())
                                        select p).ToList();

            var context = new ActivationContext()
            {
                PluginConfigurations = pluginConfigurations.ToViewModel()
            };

            return context;
        }

        private async Task<IPluginConfiguration> OnInstallPlugin()
        {
            var packagePath = await _askFile.Handle(Unit.Default);
            var config = _pluginInstaller.Install(packagePath);
            _notification.Information($"Install plugin at '{packagePath}'");
            return config;
        }

        private void OnRestart() => _restart.Restart();

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