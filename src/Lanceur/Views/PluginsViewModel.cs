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

namespace Lanceur.Views;

public class PluginsViewModel : RoutableViewModel
{
    #region Fields

    private readonly Interaction<Unit, string> _askFile;
    private readonly Interaction<Unit, string> _askWebFile;
    private readonly INotification _notification;
    private readonly IPluginInstaller _pluginInstaller;
    private readonly IPluginManifestRepository _pluginManifestRepository;
    private readonly IAppRestart _restart;
    private readonly IPluginUninstaller _uninstaller;

    #endregion Fields

    #region Constructors

    public PluginsViewModel(
        ISchedulerProvider schedulers = null,
        IUserNotification notify = null,
        IPluginUninstaller uninstaller = null,
        IPluginManifestRepository pluginConfigRepository = null,
        INotification notification = null,
        IAppRestart restart = null,
        IPluginInstaller pluginInstaller = null)
    {
        var l = Locator.Current;

        notify ??= l.GetService<IUserNotification>();

        var schedulers1 = schedulers ?? l.GetService<ISchedulerProvider>();
        _uninstaller              = uninstaller            ?? l.GetService<IPluginUninstaller>();
        _pluginManifestRepository = pluginConfigRepository ?? l.GetService<IPluginManifestRepository>();
        _notification             = notification           ?? l.GetService<INotification>();
        _restart                  = restart                ?? l.GetService<IAppRestart>();
        _pluginInstaller          = pluginInstaller        ?? l.GetService<IPluginInstaller>();

        _askFile    = Interactions.SelectFile(schedulers1.MainThreadScheduler);
        _askWebFile = new();

        Activate = ReactiveCommand.Create(OnActivate,
                                          outputScheduler: schedulers1.MainThreadScheduler);
        Activate.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

        InstallPlugin =
            ReactiveCommand.CreateFromTask(OnInstallPluginAsync, outputScheduler: schedulers1.MainThreadScheduler);
        InstallPlugin.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

        InstallPluginFromWeb = ReactiveCommand.CreateFromTask(OnInstallPluginFromWebAsync,
                                                              outputScheduler: schedulers1.MainThreadScheduler);
        InstallPluginFromWeb.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

        Restart = ReactiveCommand.Create(OnRestart, outputScheduler: schedulers1.MainThreadScheduler);
        Restart.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

        this.WhenAnyObservable(vm => vm.Activate)
            .ObserveOn(schedulers1.MainThreadScheduler)
            .Subscribe(response => { PluginManifests = new(response.PluginManifests); });

        this.WhenAnyObservable(
                vm => vm.InstallPlugin,
                vm => vm.InstallPluginFromWeb)
            .ObserveOn(schedulers1.MainThreadScheduler)
            .Where(response => response is not null)
            .Subscribe(response =>
            {
                var viewmodel = response.ToViewModel();
                RegisterInteraction(viewmodel);
                PluginManifests.Add(viewmodel);
            });
    }

    #endregion Constructors

    #region Properties

    public ReactiveCommand<Unit, ActivationContext> Activate { get; private set; }
    public Interaction<Unit, string> AskFile => _askFile;
    public Interaction<Unit, string> AskWebFile => _askWebFile;
    public ReactiveCommand<Unit, IPluginManifest> InstallPlugin { get; set; }
    public ReactiveCommand<Unit, IPluginManifest> InstallPluginFromWeb { get; set; }
    [Reactive] public ObservableCollection<PluginManifestViewModel> PluginManifests { get; private set; }
    public Action<PluginManifestViewModel> RegisterInteraction { get; set; }
    public ReactiveCommand<Unit, Unit> Restart { get; }

    #endregion Properties

    #region Methods

    private ActivationContext OnActivate()
    {
        // Get all installed plugins
        var allPluginManifests = _pluginManifestRepository.GetPluginManifests()
                                                          .ToViewModel();

        // Get all candidates for uninstall
        // and remove them from the list
        var pluginManifests = (from p in allPluginManifests
                               where !(from c in _uninstaller.UninstallationCandidates
                                       select c.Dll.GetDirectoryName()
                                   ).Contains(p.Dll.GetDirectoryName())
                               select p).ToList();

        var manifests = pluginManifests.ToViewModel();
        foreach (var manifest in manifests) RegisterInteraction(manifest);

        var context = new ActivationContext()
        {
            PluginManifests = manifests
        };

        return context;
    }

    private async Task<IPluginManifest> OnInstallPluginAsync()
    {
        var packagePath = await _askFile.Handle(Unit.Default);
        if (packagePath.IsNullOrWhiteSpace()) return null;

        var installationResult = await _pluginInstaller.InstallAsync(packagePath);
        if (!installationResult.IsInstallationSuccess)
        {
            _notification.Warning(installationResult.ErrorMessage);
            return null;
        }

        if (installationResult.IsUpdate)
        {
            _notification.Information($"Update plugin at '{packagePath}'");
            foreach (var manifest in PluginManifests.Where(x => x.Dll == installationResult.PluginManifest.Dll))
            {
                manifest.IsVisible = false;
            }
            return installationResult.PluginManifest;
        }

        _notification.Information($"Install plugin to '{packagePath}'");
        return installationResult.PluginManifest;
    }

    private async Task<IPluginManifest> OnInstallPluginFromWebAsync()
    {
        var packagePath = await _askWebFile.Handle(Unit.Default);
        if (packagePath.IsNullOrWhiteSpace()) return null;

        var config = await _pluginInstaller.InstallFromWebAsync(packagePath);
        _notification.Information($"Install plugin at '{packagePath}'");
        return config.PluginManifest;
    }

    private void OnRestart() => _restart.Restart();

    #endregion Methods

    #region Classes

    public class ActivationContext
    {
        #region Properties

        public IEnumerable<PluginManifestViewModel> PluginManifests { get; internal init; }

        #endregion Properties
    }

    #endregion Classes
}