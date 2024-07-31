using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly MostUsedViewModel _mostUsedVm;
        private readonly PluginsViewModel _pluginsViewModel;
        private readonly IDbRepository _service;
        private readonly TrendsViewModel _trendsVm;
        public readonly AppSettingsViewModel _settingsVm;

        #endregion Fields

        #region Constructors

        public SettingsViewModel(
            ILoggerFactory logFactory = null,
            KeywordsViewModel keywordVm = null,
            AppSettingsViewModel settingsVm = null,
            DoubloonsViewModel doubloonsViewModel = null,
            InvalidAliasViewModel invalidAliasVm = null,
            TrendsViewModel trendsViewModel = null,
            MostUsedViewModel mostUsedViewModel = null,
            HistoryViewModel historyViewModel = null,
            PluginsViewModel pluginsViewModel = null,
            IUserNotification notify = null,
            IDbRepository service = null)
        {
            var l = Locator.Current;
            Router = l.GetService<RoutingState>();

            notify ??= l.GetService<IUserNotification>();
            _logger = logFactory.GetLogger<SettingsViewModel>();
            _keywordVm = keywordVm ?? l.GetService<KeywordsViewModel>();
            _settingsVm = settingsVm ?? l.GetService<AppSettingsViewModel>();
            _doubloonsVm = doubloonsViewModel ?? l.GetService<DoubloonsViewModel>();
            _invalidAliasVm = invalidAliasVm ?? l.GetService<InvalidAliasViewModel>();
            _trendsVm = trendsViewModel ?? l.GetService<TrendsViewModel>();
            _mostUsedVm = mostUsedViewModel ?? l.GetService<MostUsedViewModel>();
            _historyVm = historyViewModel ?? l.GetService<HistoryViewModel>();
            _pluginsViewModel = pluginsViewModel ?? l.GetService<PluginsViewModel>();
            _service = service ?? l.GetService<IDbRepository>();

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
            _keywordVm.AliasToCreate = AliasQueryResult.FromName(aliasName);
            _logger.LogDebug("Request creation of alias {AliasName}", aliasName);
            return Router.Navigate.Execute(_keywordVm);
        }

        private IObservable<IRoutableViewModel> OnPushNavigation(string route)
        {
            route ??= "<null>";
            _logger.LogInformation("Navigate to {Route}", route);
            switch (route.ToLower())
            {
                case "keywordsview":
                    var sessionName = _service.GetDefaultSession()?.FullName ?? "N.A.";
                    Title = $"Manage aliases of '{sessionName}'";
                    return Router.Navigate.Execute(_keywordVm);

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
                    Title = $"The view '{route}' do not exist.";
                    return Router.Navigate.Execute(_settingsVm);
            }
        }

        #endregion Methods
    }
}