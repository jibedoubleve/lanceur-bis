using ReactiveUI;
using ScottPlot;
using ScottPlot.Statistics.Interpolation;
using System;
using System.Drawing;
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
                    var d = days.ToArray();
                    var v = values.ToArray();

                    History.Plot.XAxis.TickLabelFormat("dd/MM/yyyy", dateTimeFormat: true);
                    History.Plot.XAxis.Label("Day");

                    History.Plot.YAxis.Label("Usage count");
                    History.Plot.YAxis.ManualTickSpacing(5);
                    History.Plot.Style(ScottPlot.Style.Gray1);


                    History.Plot.XAxis.TickLabelNotation(multiplier: true);
                    History.Plot.YAxis.TickLabelNotation(multiplier: true);

                    var scatter = History.Plot.AddScatter(d, v, Color.Crimson, label: "Usage");
                    scatter.MarkerShape = MarkerShape.none;
                    History.Plot.Legend();
                    History.Refresh();
                };

                ViewModel.Activate.Execute().Subscribe();
            });

        }

        #endregion Constructors
    }
}