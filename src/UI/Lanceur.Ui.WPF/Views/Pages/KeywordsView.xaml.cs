using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class KeywordsView : IDisposable
{
    #region Fields

    private readonly CodeEditorView _codeEditorView;

    private readonly IContentDialogService _contentDialogService;

    #endregion

    #region Constructors

    public KeywordsView(
        KeywordsViewModel viewModel,
        IContentDialogService contentDialogService,
        CodeEditorView codeEditorView
    )
    {
        WeakReferenceMessenger.Default.Register<KeywordsView, QuestionRequestMessage>(this, (_, m) => m.Reply(HandleMessageBoxAsync(m)));
        InitializeComponent();
        _contentDialogService = contentDialogService;
        _codeEditorView = codeEditorView;
        DataContext = ViewModel = viewModel;
    }

    #endregion

    #region Properties

    private KeywordsViewModel ViewModel { get;  }

    #endregion

    #region Methods

    private async Task<bool> HandleMessageBoxAsync(QuestionRequestMessage request)
    {
        var result = await _contentDialogService.ShowSimpleDialogAsync(
            new() { Title = request.Title, Content = request.Content, PrimaryButtonText = request.YesText, CloseButtonText = request.NoText }
        );
        return result == ContentDialogResult.Primary;
    }

    private void OnClickCodeEditor(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedAlias is null) return;

        var viewModel = (CodeEditorViewModel)_codeEditorView.DataContext;
        viewModel.Alias = ViewModel.SelectedAlias!;
        var content = (ViewType: typeof(CodeEditorView), DataContext: viewModel);
        WeakReferenceMessenger.Default.Send<NavigationMessage>(new(content));
    }

    public void Dispose() { WeakReferenceMessenger.Default.Unregister<KeywordsView>(this); }

    #endregion
}