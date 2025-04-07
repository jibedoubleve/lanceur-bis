using System.ComponentModel;
using System.Windows;
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
        viewModel.PropertyChanged += OnViewModelReportTypeChanged;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private DataReconciliationViewModel ViewModel => (DataReconciliationViewModel)DataContext;

    #endregion

    #region Methods

    private void OnViewModelReportTypeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.ReportType)) return;

        switch (ViewModel)
        {
            case { ReportType: ReportType.UnannotatedAliases }:
            case {ReportType: ReportType.RestoreAlias}:
                ColumnParameters.Visibility = Visibility.Collapsed;
                ColumnFileName.Visibility = Visibility.Collapsed;
                ColumnProposedDescription.Visibility = Visibility.Collapsed;
                ColumnLastUsed.Visibility = Visibility.Collapsed;
                ColumnUsageCount.Visibility = Visibility.Collapsed;
                break;
            case {ReportType: ReportType.InactiveAliases}:
                ColumnProposedDescription.Visibility = Visibility.Collapsed;
                ColumnLastUsed.Visibility = Visibility.Visible;
                ColumnUsageCount.Visibility = Visibility.Collapsed;
                break;
            case {ReportType: ReportType.RarelyUsedAliases}: 
                ColumnProposedDescription.Visibility = Visibility.Collapsed;
                ColumnLastUsed.Visibility = Visibility.Collapsed;
                ColumnUsageCount.Visibility = Visibility.Visible;
                break;
            default:
                ColumnParameters.Visibility = Visibility.Visible;
                ColumnFileName.Visibility = Visibility.Visible;
                ColumnProposedDescription.Visibility = Visibility.Collapsed;
                ColumnLastUsed.Visibility = Visibility.Collapsed;
                ColumnUsageCount.Visibility = Visibility.Collapsed;
                break;
        }
    }

    #endregion
}