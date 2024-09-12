using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class KeywordsView :IDisposable
{
    private readonly IContentDialogService _contentDialogService;

    public KeywordsView(KeywordsViewModel viewModel, IContentDialogService contentDialogService)
    {
        _contentDialogService = contentDialogService;
        DataContext = viewModel;
        WeakReferenceMessenger.Default.Register<KeywordsView, AskDeleteAlias>(this, (_, m) => m.Reply(HandleMessageBoxAsync(m)));
        InitializeComponent();
    }

    private async Task<bool> HandleMessageBoxAsync(AskDeleteAlias request)
    {
        var result = await _contentDialogService.ShowSimpleDialogAsync(
            new()
            {
                Title = "DELETE", 
                Content = $"Do you want to delete the alias '{request.AliasName}'?", 
                PrimaryButtonText = "Delete", 
                CloseButtonText = "Cancel"
            }
        );
        return result == ContentDialogResult.Primary;
    }

    public void Dispose() { WeakReferenceMessenger.Default.Unregister<KeywordsView>(this); }
}