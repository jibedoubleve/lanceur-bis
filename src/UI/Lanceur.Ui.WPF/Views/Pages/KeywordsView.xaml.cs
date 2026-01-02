using System.Windows;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Views.Controls;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class KeywordsView
{
    #region Fields

    private readonly CodeEditorControl _codeEditorControl;
    private readonly IContentDialogService _contentDialogService;
    private readonly ILogger<KeywordsView> _logger;
    private readonly IUserCommunicationService _userCommunicationService;

    #endregion

    #region Constructors

    public KeywordsView(
        KeywordsViewModel viewModel,
        CodeEditorControl codeEditorControl,
        IContentDialogService contentDialogService,
        ILogger<KeywordsView> logger,
        IUserCommunicationService  userCommunicationService)
    {
        _codeEditorControl = codeEditorControl;
        _contentDialogService = contentDialogService;
        _logger = logger;
        _userCommunicationService = userCommunicationService;
        DataContext = ViewModel = viewModel;

        InitializeComponent();
    }

    #endregion

    #region Properties

    private KeywordsViewModel ViewModel { get;  }

    #endregion

    #region Methods

    private async void OnClickCodeEditor(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ViewModel.SelectedAlias is null) return;

            _codeEditorControl.Load(ViewModel.SelectedAlias);

            var dialogue = new ContentDialog
            {
                Title = "Edit script",
                Content = _codeEditorControl,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Cancel",
            };

            var result = await _contentDialogService.ShowAsync(dialogue, default);

            ViewModel.SelectedAlias.Script = result == ContentDialogResult.Primary
                ? _codeEditorControl.Apply()
                : _codeEditorControl.Reset();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load the lua script editor: {ErrorMessage}", ex.Message);
            _userCommunicationService.Notifications.Warning($"Failed to load the lua script editor: {ex.Message}");
        }
    }

    #endregion
}