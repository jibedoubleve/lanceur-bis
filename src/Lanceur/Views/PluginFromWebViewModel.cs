using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Views.Mixins;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views
{
    public class PluginFromWebViewModel : ReactiveObject
    {
        #region Fields

        private readonly IPluginWebRepository _webRepository;
        private readonly IPluginUninstaller _pluginUninstaller;
        #endregion Fields

        #region Constructors

        public PluginFromWebViewModel(IPluginUninstaller pluginUninstaller = null, 
                                      ISchedulerProvider schedulers = null,
                                      IUserNotification notify = null,
                                      IPluginWebRepository webRepository = null)
        {
            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();
            schedulers ??= l.GetService<ISchedulerProvider>();
            _pluginUninstaller = pluginUninstaller ?? l.GetService<IPluginUninstaller>();
            _webRepository = webRepository ?? l.GetService<IPluginWebRepository>();
            Activate = ReactiveCommand.CreateFromTask(OnActivateAsync, outputScheduler: schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(schedulers.MainThreadScheduler)
                .Subscribe(response =>
                {
                    PluginManifests = new(response.PluginManifests);
                });
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ActivationResponse> Activate { get;  }
        [Reactive] public ObservableCollection<PluginWebManifestViewModel> PluginManifests { get; set; }
        [Reactive] public PluginWebManifestViewModel SelectedManifest { get; set; }
        [Reactive] public bool SelectionValidated { get; set; } = false;

        #endregion Properties

        #region Methods

        private async Task<ActivationResponse> OnActivateAsync()
        {
            var manifests = await _webRepository.GetPluginListAsync(_pluginUninstaller.UninstallationCandidates);
            return new() { PluginManifests = manifests.ToViewModel() };
        }

        #endregion Methods

        #region Classes

        public class ActivationResponse
        {
            #region Properties

            public IEnumerable<PluginWebManifestViewModel> PluginManifests { get; init; }

            #endregion Properties
        }

        #endregion Classes
    }
}