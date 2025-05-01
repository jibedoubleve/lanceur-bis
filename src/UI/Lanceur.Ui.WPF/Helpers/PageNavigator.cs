using System.Windows;
using System.Windows.Controls;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Helpers;

public class PageNavigator
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PageNavigator> _logger;

    #endregion

    #region Constructors

    public PageNavigator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService<ILogger<PageNavigator>>()!;
    }

    #endregion

    #region Methods

    public void NavigateToSettings<TView>() where TView : Page
    {
        var windows = Application.Current.Windows.OfType<SettingsView>().ToArray();
        var view  = windows.Length != 0
            ? windows[0]
            : _serviceProvider.GetService<SettingsView>()!;
        
        _logger.LogDebug("Navigating to settings subview {View}", view.GetType().FullName);
        view.Show();
        view.Navigate<TView>();
        
    }

    #endregion
}