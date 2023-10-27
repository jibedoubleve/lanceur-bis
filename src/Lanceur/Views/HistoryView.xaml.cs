using Lanceur.Utils;
using ReactiveUI;
using System;
using System.Linq;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for HistoryView.xaml
    /// </summary>
    public partial class HistoryView : IViewFor<HistoryViewModel>
    {
        #region Constructors

        public HistoryView()
        {
            InitializeComponent();

            this.WhenActivated(disposable =>
            {
                ViewModel.OnRefreshChart = (days, values) =>
                {
                    if (days.Any() && values.Any())
                    {
                        var d = days.ToArray();
                        var v = values.ToArray();

                        History.Plot.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                        History.Plot.XAxis.Label("Day");
                        History.Plot.XAxis.TickLabelNotation(multiplier: true);

                        History.Plot.YAxis.Label("Usage counter");
                        History.Plot.YAxis.TickLabelFormat("N0", dateTimeFormat: false);
                        History.Plot.YAxis.TickLabelNotation(multiplier: false);

                        History.Plot.Style(ScottPlot.Style.Gray1);
                        History.Plot.AddBar(v, d);
                        History.Plot.Legend();
                        History.Refresh();
                    }
                    else { AppLogFactory.Get<HistoryView>().Warning("No history to display."); }
                };

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors
    }
}