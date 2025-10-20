using System.ComponentModel;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Repositories.Config;
using Lanceur.SharedKernel.DI;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Helpers;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class DataReconciliationView
{
    #region Fields

    private readonly IEnumerable<ReportConfiguration> _reportConfigurations;

    #endregion

    #region Constructors

    public DataReconciliationView(
        DataReconciliationViewModel viewModel,
        IConfigurationFacade configuration
    )
    {
        _reportConfigurations = configuration.Application.Reconciliation.ReportsConfiguration;
        DataContext = viewModel;
        viewModel.PropertyChanged += OnViewModelReportTypeChanged;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private DataReconciliationViewModel ViewModel => (DataReconciliationViewModel)DataContext;

    #endregion

    #region Methods

    private void HandleColumnVisibility(string? propertyName)
    {
        if (propertyName != "CurrentReportConfiguration") return;

        ColumnLastUsed.Visibility = new BoolToVisibility(
            ViewModel.CurrentReportConfiguration.ColumnsVisibility.LastUsed
        );
        ColumnUsageCount.Visibility = new BoolToVisibility(
            ViewModel.CurrentReportConfiguration.ColumnsVisibility.UsageCount
        );
        ColumnFileName.Visibility = new BoolToVisibility(
            ViewModel.CurrentReportConfiguration.ColumnsVisibility.FileName
        );
        ColumnParameters.Visibility = new BoolToVisibility(
            ViewModel.CurrentReportConfiguration.ColumnsVisibility.Parameters
        );
        ColumnProposedDescription.Visibility = new BoolToVisibility(
            ViewModel.CurrentReportConfiguration.ColumnsVisibility.ProposedDescription
        );
    }

    private void OnViewModelReportTypeChanged(object? sender, PropertyChangedEventArgs e)
    {
        HandleColumnVisibility(e.PropertyName);
        if (e.PropertyName != nameof(ViewModel.ReportType)) return;

        var cfg = _reportConfigurations.SingleOrDefault(r => r.ReportType == ViewModel.ReportType);
        if (cfg is null) return;

        ColumnProposedDescription.Visibility = new BoolToVisibility(cfg.ColumnsVisibility.ProposedDescription);
        ColumnLastUsed.Visibility = new BoolToVisibility(cfg.ColumnsVisibility.LastUsed);
        ColumnUsageCount.Visibility = new BoolToVisibility(cfg.ColumnsVisibility.UsageCount);
        ColumnParameters.Visibility = new BoolToVisibility(cfg.ColumnsVisibility.Parameters);
        ColumnFileName.Visibility = new BoolToVisibility(cfg.ColumnsVisibility.FileName);
    }

    #endregion
}