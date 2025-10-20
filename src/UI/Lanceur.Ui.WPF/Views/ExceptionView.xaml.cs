using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Lanceur.Core.Constants;
using Lanceur.Core.Repositories.Config;
using Lanceur.Ui.Core.ViewModels;
using Lanceur.Ui.WPF.Extensions;
using Wpf.Ui.Appearance;

namespace Lanceur.Ui.WPF.Views;

/// <summary>
///     Interaction logic for ExceptionViewer.xaml
/// </summary>
public partial class ExceptionView
{
    #region Fields

    private readonly IConfigurationFacade _configuration;

    #endregion

    #region Constructors

    public ExceptionView(
        ExceptionViewModel viewModel,
        IConfigurationFacade configuration
    )
    {
        DataContext = viewModel;

        InitializeComponent();
        _configuration = configuration;
    }

    #endregion

    #region Methods

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private void OnLoaded(object _, RoutedEventArgs e)
    {
        SystemThemeWatcher.Watch(
            this,
            _configuration.Application.Window.BackdropStyle.ToWindowBackdropType()
        );
    }

    private void OnOpenLogs(object sender, RoutedEventArgs e) => Process.Start("explorer.exe", Paths.LogRepository);

    #endregion
}