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

                ViewModel.ConfirmRemove.RegisterHandler(async interaction =>
                {
                    var value = 0L;
                    long.TryParse(interaction.Input, out value);

                    var result = await Dialogs.YesNoQuestion($"Do you want to delete {interaction.Input} {(value > 1 ? "aliases" : "alias")}?");
                    interaction.SetOutput(result.ToBool());
                });

                ViewModel.Activate.Execute().Subscribe();
            });
        }

        #endregion Constructors
    }
}