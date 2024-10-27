using CommunityToolkit.Mvvm.Messaging;
using Lanceur.SharedKernel.DI;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class DataReconciliationView
{
    #region Fields

    private readonly IContentDialogService _contentDialogService;

    #endregion

    #region Constructors

    public DataReconciliationView(DataReconciliationViewModel viewModel, IContentDialogService contentDialogService)
    {
        _contentDialogService = contentDialogService;
        DataContext = viewModel;
        WeakReferenceMessenger.Default.Register<DataReconciliationView, QuestionRequestMessage>(this, (_, m) => m.Reply(HandleMessageBoxAsync(m)));
        InitializeComponent();
    }

    #endregion

    #region Methods

    private async Task<bool> HandleMessageBoxAsync(QuestionRequestMessage request)
    {
        var result = await _contentDialogService.ShowSimpleDialogAsync(
            new()
            {
                Title = request.Title,
                Content = request.Content,
                PrimaryButtonText = request.YesText,
                CloseButtonText = request.NoText,                
            }
        );
        return result == ContentDialogResult.Primary;
    }

    #endregion
}