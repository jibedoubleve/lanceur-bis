using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.SharedKernel.DI;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class KeywordsView 
{
    #region Fields

    private readonly CodeEditorView _codeEditorView;

    #endregion

    #region Constructors

    public KeywordsView(
        KeywordsViewModel viewModel,
        CodeEditorView codeEditorView
    )
    {
        _codeEditorView = codeEditorView;

        DataContext = ViewModel = viewModel;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private KeywordsViewModel ViewModel { get;  }

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

    #endregion
}