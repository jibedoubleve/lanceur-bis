using Lanceur.Ui;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Lanceur.Views;

/// <summary>
/// Interaction logic for InvalidAliasView.xaml
/// </summary>
public partial class InvalidAliasView : IViewFor<InvalidAliasViewModel>
{
    #region Constructors

    public InvalidAliasView()
    {
        InitializeComponent();

        this.WhenActivated(
            d =>
            {
                this.OneWayBind(ViewModel, vm => vm.InvalidAliases, v => v.InvalidAliases.ItemsSource).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveSelected, v => v.BtnRemoveSelected).DisposeWith(d);

                ViewModel.ConfirmRemove.RegisterHandler(
                    async interaction =>
                    {
                        var value = 0L;
                        long.TryParse(interaction.Input, out value);

                        var result = await Dialogs.YesNoQuestion($"Do you want to delete {interaction.Input} {(value > 1 ? "aliases" : "alias")}?");
                        interaction.SetOutput(result.ToBool());
                    }
                );

                ViewModel.Activate.Execute().Subscribe();
            }
        );
    }

    #endregion Constructors
}