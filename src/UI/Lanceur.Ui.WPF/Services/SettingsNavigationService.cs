using System.Windows;
using Lanceur.Core.Services;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Views;
using Lanceur.Ui.WPF.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Services;

/// <inheritdoc />
public sealed class SettingsNavigationService : ISettingsNavigationService
{
    #region Fields

    private static readonly Dictionary<Type, Action<SettingsView>> Pages = new()
    {
        [typeof(AnalyticsViewModel)] = view => view.Navigate<AnalyticsView>(),
        [typeof(KeywordsViewModel)] = view => view.Navigate<KeywordsView>()
    };

    private readonly ILogger<SettingsNavigationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public SettingsNavigationService(IServiceProvider serviceProvider, ILogger<SettingsNavigationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        if (!Pages.TryGetValue(typeof(TViewModel), out var navigate))
        {
            throw new NotSupportedException($"No settings page is bound to {typeof(TViewModel).Name}.");
        }

        var windows = Application.Current.Windows.OfType<SettingsView>().ToArray();
        var view = windows.Length != 0
            ? windows[0]
            : _serviceProvider.GetRequiredService<SettingsView>();

        _logger.LogDebug("Navigating to the settings page of {ViewModel}", typeof(TViewModel).Name);
        view.Show();
        navigate(view);
    }

    #endregion
}
