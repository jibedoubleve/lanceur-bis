using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for MostUsedView.xaml
    /// </summary>
    public partial class MostUsedView : IViewFor<MostUsedViewModel>
    {
        #region Constructors

        public MostUsedView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Aliases, v => v.Aliases.ItemsSource).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors
    }
}