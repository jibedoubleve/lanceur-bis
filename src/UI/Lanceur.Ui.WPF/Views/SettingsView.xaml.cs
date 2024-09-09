using System.Windows;
using System.Windows.Controls;
using Lanceur.Ui.WPF.Views.Pages;
using Wpf.Ui;

namespace Lanceur.Ui.WPF.Views;

public partial class SettingsView
{
    #region Constructors

    public SettingsView(IPageService pageService)
    {
        InitializeComponent();
        PageNavigationView.SetPageService(pageService);
    }

    #endregion

    #region Methods

    public void Navigate<T>() where T : Page => PageNavigationView.Navigate(typeof(KeywordsView));

    #endregion
}