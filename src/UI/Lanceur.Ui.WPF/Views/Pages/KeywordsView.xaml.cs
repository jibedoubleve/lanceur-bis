using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class KeywordsView
{
    #region Fields

    private readonly LuaEditorView _luaEditorView;
    private readonly ILogger<KeywordsView> _logger;
    private readonly IUserCommunicationService _userCommunicationService;

    #endregion

    #region Constructors

    public KeywordsView(
        KeywordsViewModel viewModel,
        LuaEditorView luaEditorView,
        ILogger<KeywordsView> logger,
        IUserCommunicationService userCommunicationService)
    {
        _luaEditorView = luaEditorView;
        _logger = logger;
        _userCommunicationService = userCommunicationService;
        DataContext = ViewModel = viewModel;

        InitializeComponent();
    }

    #endregion

    #region Properties

    private KeywordsViewModel ViewModel { get; }

    #endregion

    #region Methods

    private void OnClickCodeEditor(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ViewModel.SelectedAlias is null) return;

            _luaEditorView.LoadAlias(ViewModel.SelectedAlias);

            WeakReferenceMessenger.Default.Send(
                new NavigationMessage((typeof(LuaEditorView), null))
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load the lua script editor: {ErrorMessage}", ex.Message);
            _userCommunicationService.Notifications.Warning($"Failed to load the lua script editor: {ex.Message}");
        }
    }

    #endregion
}