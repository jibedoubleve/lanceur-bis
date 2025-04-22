using System.Diagnostics;
using System.Windows;
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

    private readonly ISettingsFacade _settings;

    #endregion

    #region Constructors

    public ExceptionView(
        ExceptionViewModel viewModel,
        ISettingsFacade settings
    )
    {
        DataContext = viewModel;

        InitializeComponent();
        _settings = settings;
    }

    #endregion

    #region Methods

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnLoaded(object _, RoutedEventArgs e)
    {
        SystemThemeWatcher.Watch(
            this,
            _settings.Application.Window.BackdropStyle.ToWindowBackdropType()
        );
    }

    private void OnOpenLogs(object sender, RoutedEventArgs e) => Process.Start("explorer.exe", Paths.LogRepository);

    #endregion
}