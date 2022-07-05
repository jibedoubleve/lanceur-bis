using ReactiveUI;
using ScottPlot;
using ScottPlot.Statistics.Interpolation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for TrendsView.xaml
    /// </summary>
    public partial class TrendsView : IViewFor<TrendsViewModel>
    {
        #region Constructors

        public TrendsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel.OnRefreshChartHour = (days, values) => SetChart(Hour, days, values);
                ViewModel.OnRefreshChartDayOfWeek = (days, values) => SetChart(DayOfWeek, days, values);
                ViewModel.OnRefreshChartMonth = (days, values) => SetChart(Month, days, values);

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors

        #region Methods

        private static void SetChart(WpfPlot ctrl, IEnumerable<string> days, IEnumerable<double> values)
        {
            var d = days.ToArray();
            var v = values.ToArray();

            ctrl.Plot.Style(ScottPlot.Style.Gray1);
            double[] positions = Enumerable.Range(0, v.Length).Select(x => (double)x).ToArray();

            ctrl.Plot.AddBar(v, positions);
            ctrl.Plot.XTicks(positions, d);

            ctrl.Plot.Legend();
            ctrl.Refresh();
        }

        #endregion Methods
    }
}