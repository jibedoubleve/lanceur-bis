using Lanceur.Ui;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for DoubloonsView.xaml
    /// </summary>
    public partial class DoubloonsView : IViewFor<DoubloonsViewModel>
    {
        #region Constructors

        public DoubloonsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Doubloons, v => v.Doubloons.ItemsSource).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveSelected, v => v.BtnRemoveSelected).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors
    }
}