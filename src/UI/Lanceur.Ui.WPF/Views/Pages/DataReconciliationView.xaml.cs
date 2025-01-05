using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.SharedKernel.DI;
using Lanceur.Ui.Core.ViewModels.Pages;
using Wpf.Ui;

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
        viewModel.PropertyChanged += OnViewModelOnPropertyChanged;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private DataReconciliationViewModel ViewModel => (DataReconciliationViewModel)DataContext;

    #endregion

    #region Methods

    private void OnViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.ReportType)) return;

        switch (ViewModel)
        {
            case { ReportType: ReportType.UnannotatedAliases }:
            case {ReportType: ReportType.RestoreAlias}:
                ColumnParameters.Visibility = Visibility.Collapsed;
                ColumnFileName.Visibility = Visibility.Collapsed;
                ColumnProposedDescription.Visibility = Visibility.Visible;
                break;
            default:
                ColumnParameters.Visibility = Visibility.Visible;
                ColumnFileName.Visibility = Visibility.Visible;
                ColumnProposedDescription.Visibility = Visibility.Collapsed;
                break;
        }
    }

    #endregion
}