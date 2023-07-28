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

        private readonly ISchedulerProvider _schedulers;
        private readonly IPluginWebRepository _webRepositopry;

        #endregion Fields

        #region Constructors

        public PluginFromWebViewModel(
            ISchedulerProvider schedulers = null,
            IUserNotification notify = null,
            IPluginWebRepository webRepositopry = null)
        {
            var l = Locator.Current;
            notify ??= l.GetService<IUserNotification>();

            _schedulers = schedulers ?? l.GetService<ISchedulerProvider>();
            _webRepositopry = webRepositopry ?? l.GetService<IPluginWebRepository>();
            Activate = ReactiveCommand.CreateFromTask(OnActivateAsync, outputScheduler: _schedulers.MainThreadScheduler);
            Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            this.WhenAnyObservable(vm => vm.Activate)
                .ObserveOn(_schedulers.MainThreadScheduler)
                .Subscribe(response =>
                {
                    PluginManifests = new ObservableCollection<PluginWebManifestViewModel>(response.PluginManifests);
                });
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<Unit, ActivationResponse> Activate { get; private set; }
        [Reactive] public ObservableCollection<PluginWebManifestViewModel> PluginManifests { get; set; }
        [Reactive] public PluginWebManifestViewModel SelectedManifest { get; set; }
        [Reactive] public bool SelectionValidated { get; set; } = false;

        #endregion Properties

        #region Methods

        private async Task<ActivationResponse> OnActivateAsync()
        {
            var manifests = await _webRepositopry.GetPluginListAsync();
            return new() { PluginManifests = manifests.ToViewModel() };
        }

        #endregion Methods

        #region Classes

        public class ActivationResponse
        {
            #region Properties

            public IEnumerable<PluginWebManifestViewModel> PluginManifests { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}