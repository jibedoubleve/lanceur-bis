using Lanceur.Core.Plugins;
using Lanceur.Core.Services;
using Lanceur.Schedulers;
using Lanceur.Ui;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lanceur.Views;

public class PluginManifestViewModel : ReactiveObject, IPluginManifest
{
    #region Fields

    private readonly Interaction<string, bool> _confirmRemove;
    private readonly IPluginUninstaller _pluginUninstaller;

    #endregion Fields

    #region Constructors

    public PluginManifestViewModel(
        ISchedulerProvider schedulerProvider = null,
        IUserNotification notify = null,
        IPluginUninstaller pluginUninstaller = null
    )
    {
        var l = Locator.Current;
        notify ??= l.GetService<IUserNotification>();
        _pluginUninstaller = pluginUninstaller ?? l.GetService<IPluginUninstaller>();
        var schedulers = schedulerProvider ?? new RxAppSchedulerProvider();

        _confirmRemove = Interactions.YesNoQuestion(schedulers.MainThreadScheduler);

        Uninstall = ReactiveCommand.CreateFromTask(OnUninstall, outputScheduler: schedulers.MainThreadScheduler);
        Uninstall.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

        this.WhenAnyObservable(vm => vm.Uninstall)
            .BindTo(this, vm => vm.IsVisible);
    }

    #endregion Constructors

    #region Properties

    public Version AppMinVersion { get; set; }
    public string Author { get; set; }
    public Interaction<string, bool> ConfirmRemove => _confirmRemove;
    public string Description { get; set; }

    public string Dll { get; set; }

    public string Help { get; set; }

    [Reactive] public bool IsVisible { get; set; } = true;

    public string Name { get; set; }

    public ReactiveCommand<Unit, bool> Uninstall { get; set; }
    public Version Version { get; set; }

    #endregion Properties

    #region Methods

    private async Task<bool> OnUninstall()
    {
        if (false == await _confirmRemove.Handle(Name)) return true;

        await _pluginUninstaller.SubscribeForUninstallAsync(this);
        return false;
    }

    #endregion Methods
}