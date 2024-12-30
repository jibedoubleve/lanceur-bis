using System.Windows;
using System.Windows.Controls;
using Lanceur.Ui.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace Lanceur.Ui.WPF.Helpers;

public class PageNavigator
{
    #region Fields

    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Constructors

    public PageNavigator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    #endregion

    #region Methods

    public void Navigate<TView>() where TView : Page
    {
        var windows = Application.Current.Windows.OfType<SettingsView>().ToArray();
        var view  = windows.Any()
            ? windows.ElementAt(0)
            : _serviceProvider.GetService<SettingsView>()!;
        view.Show();
        view.Navigate<TView>();
    }

    #endregion
}