using Lanceur.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;

namespace Lanceur.Views
{
    public class SettingsViewModel : ReactiveObject, IScreen
    {
        #region Fields

        private readonly DoubloonsViewModel _doubloonsVm;
        private readonly HistoryViewModel _historyVm;
        private readonly InvalidAliasViewModel _invalidAliasVm;
        private readonly KeywordsViewModel _keywordVm;
        private readonly ILogService _log;
        private readonly MostUsedViewModel _mostUsedVm;
        private readonly PluginsViewModel _pluginsViewModel;
        private readonly IDataService _service;
        private readonly SessionsViewModel _sessionsVm;
        private readonly TrendsViewModel _trendsVm;
        public readonly AppSettingsViewModel _settingsVm;

        #endregion Fields

        #region Constructors

        public SettingsViewModel(
            ILogService log = null,
            KeywordsViewModel keywordVm = null,
            SessionsViewModel sessionsVm = null,
            AppSettingsViewModel settingsVm = null,
            DoubloonsViewModel doubloonsViewModel = null,
            InvalidAliasViewModel invalidAliasVm = null,
            TrendsViewModel trendsViewModel = null,
            MostUsedViewModel mostUsedViewModel = null,
            HistoryViewModel historyViewModel = null,
            PluginsViewModel pluginsViewModel = null,
            IUserNotification notify = null,
            IDataService service = null)
        {
            _log ??= log;

            var l = Locator.Current;
            Router = l.GetService<RoutingState>();

            notify ??= l.GetService<IUserNotification>();
            _log = log ?? l.GetService<ILogService>();
            _keywordVm = keywordVm ?? l.GetService<KeywordsViewModel>();
            _sessionsVm = sessionsVm ?? l.GetService<SessionsViewModel>();
            _settingsVm = settingsVm ?? l.GetService<AppSettingsViewModel>();
            _doubloonsVm = doubloonsViewModel ?? l.GetService<DoubloonsViewModel>();
            _invalidAliasVm = invalidAliasVm ?? l.GetService<InvalidAliasViewModel>();
            _trendsVm = trendsViewModel ?? l.GetService<TrendsViewModel>();
            _mostUsedVm = mostUsedViewModel ?? l.GetService<MostUsedViewModel>();
            _historyVm = historyViewModel ?? l.GetService<HistoryViewModel>();
            _pluginsViewModel = pluginsViewModel ?? l.GetService<PluginsViewModel>();
            _service = service ?? l.GetService<IDataService>();

            PushNavigation = ReactiveCommand.CreateFromObservable<string, IRoutableViewModel>(OnPushNavigation);
            PushNavigation.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));

            AddAlias = ReactiveCommand.CreateFromObservable<string, IRoutableViewModel>(OnAddAlias);
            AddAlias.ThrownExceptions.Subscribe(ex => notify.Error(ex.Message, ex));
        }

        #endregion Constructors

        #region Properties

        public ReactiveCommand<string, IRoutableViewModel> AddAlias { get; }

        public ReactiveCommand<string, IRoutableViewModel> PushNavigation { get; }

        public RoutingState Router { get; }

        [Reactive] public string Title { get; set; }

        #endregion Properties

        #region Methods

        private IObservable<IRoutableViewModel> OnAddAlias(string aliasName)
        {
            _keywordVm.CreateAlias.Execute(aliasName).Subscribe();
            return Router.Navigate.Execute(_keywordVm);
        }

        private IObservable<IRoutableViewModel> OnPushNavigation(string arg)
        {
            _log.Trace($"Navigate to '{arg ?? ""}'");
            switch (arg.ToLower())
            {
                case "keywordsview":
                    var sessionName = _service.GetDefaultSession()?.FullName ?? "N.A.";
                    Title = $"Manage aliases of '{sessionName}'";
                    return Router.Navigate.Execute(_keywordVm);

                case "sessionsview":
                    Title = "Manage sessions";
                    return Router.Navigate.Execute(_sessionsVm);

                case "doubloonsview":
                    Title = "Manage doubloons";
                    return Router.Navigate.Execute(_doubloonsVm);

                case "settings":
                    Title = "Manage settings";
                    return Router.Navigate.Execute(_settingsVm);

                case "invalidaliasview":
                    Title = "Manage invalid aliases";
                    return Router.Navigate.Execute(_invalidAliasVm);

                case "trendsview":
                    Title = "Show usage trends";
                    return Router.Navigate.Execute(_trendsVm);

                case "mostusedview":
                    Title = "List of most used aliases";
                    return Router.Navigate.Execute(_mostUsedVm);

                case "historyview":
                    Title = "Usage history";
                    return Router.Navigate.Execute(_historyVm);

                case "pluginsview":
                    Title = "Manage plugins";
                    return Router.Navigate.Execute(_pluginsViewModel);

                default:
                    Title = $"The view '{arg}' do not exist.";
                    return Router.Navigate.Execute(_settingsVm);
            }
        }

        #endregion Methods
    }
}