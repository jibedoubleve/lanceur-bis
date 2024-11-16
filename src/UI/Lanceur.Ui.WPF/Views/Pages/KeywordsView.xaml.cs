using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;
using Wpf.Ui;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class KeywordsView : IDisposable
{
    #region Fields

    private readonly CodeEditorView _codeEditorView;

    #endregion

    #region Constructors

    public KeywordsView(
        KeywordsViewModel viewModel,
        IContentDialogService contentDialogService,
        CodeEditorView codeEditorView
    )
    {
        InitializeComponent();
        _codeEditorView = codeEditorView;
        DataContext = ViewModel = viewModel;
    }

    #endregion

    #region Properties

    private KeywordsViewModel ViewModel { get; init; }

    #endregion

    #region Methods

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