using System.Windows;
using System.Windows.Input;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Application;
using Lanceur.Core.Constants;
using Lanceur.Infra.Win32.Helpers;
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

    private readonly ISection<WindowSection> _windowSection;

    #endregion

    #region Constructors

    public ExceptionView(
        ExceptionViewModel viewModel,
        ISection<WindowSection> windowSection)
    {
        _windowSection = windowSection;
        DataContext = viewModel;

        InitializeComponent();
    }

    #endregion

    #region Methods

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) { Close(); }
    }

    private void OnLoaded(object _, RoutedEventArgs e) =>
        SystemThemeWatcher.Watch(
            this,
            _windowSection.Value.BackdropStyle.ToWindowBackdropType()
        );

    private void OnOpenLogs(object sender, RoutedEventArgs e) => WindowsShell.StartExplorer(Paths.LogRepository);

    #endregion
}