using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Views.Pages;

public sealed partial class AnalyticsView
{

    #region Constructors

    public AnalyticsView(AnalyticsViewModel? viewModel, ILogger<AnalyticsView> logger)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #endregion
}