using Lanceur.Converters.Reactive;
using Lanceur.Infra.Logging;
using Lanceur.Ui;
using Lanceur.Utils;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Splat;

namespace Lanceur.Views;

/// <summary>
/// Interaction logic for KeywordsView.xaml
/// </summary>
public partial class KeywordsView
{
    #region Constructors

    public KeywordsView()
    {
        InitializeComponent();
        DataContext = ViewModel;

        this.WhenActivated(
            d =>
            {
                Locator.Current.GetLogger<KeywordsView>().LogActivate<KeywordsView>();

                ViewModel.AskLuaEditor.RegisterHandler(
                    interaction =>
                    {
                        var backup = interaction.Input;
                        var window = new LuaEditorView { Owner = Window.GetWindow(this), LuaScript = interaction.Input };
                        var dialogResult = window.ShowDialog();

                        interaction.SetOutput(
                            dialogResult == true
                                ? window.LuaScript.Code
                                : backup.Code
                        );
                    }
                );

                ViewModel.ConfirmRemove.RegisterHandler(
                    async interaction =>
                    {
                        var result = await Dialogs.YesNoQuestion($"Do you want to delete alias '{interaction.Input}' and all its synonyms?");
                        interaction.SetOutput(result.ToBool());
                    }
                );


                this.OneWayBind(ViewModel, vm => vm.MacroCollection, v => v.BoxFileName.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Aliases, v => v.Aliases.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BusyMessage, v => v.BusyMessage.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.BusyControl.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.AliasList.Visibility, val => val.ToVisibilityInverted()).DisposeWith(d);
                this.OneWayBind(
                        ViewModel,
                        vm => vm.SelectedAlias,
                        v => v.BtnDeleteAlias.Content,
                        val => val is null ? "Delete" : val.Id == 0 ? "Discard" : "Delete"
                    )
                    .DisposeWith(d);

                this.Bind(ViewModel, vm => vm.SearchQuery, v => v.QueryBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedAlias, v => v.Aliases.SelectedItem).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.CreatingAlias, v => v.BtnCreateAlias).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveAlias, v => v.BtnDeleteAlias, v => v.SelectedAlias).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveOrUpdateAlias, v => v.BtnSaveOrUpdateAlias, v => v.SelectedAlias).DisposeWith(d);

                //Validations
                this.BindValidation(ViewModel, vm => vm.ValidationFileName, v => v.BoxFileNameValidation.Text).DisposeWith(d);
                this.BindValidation(ViewModel, vm => vm.ValidationAliasExists, v => v.BoxNameValidation.Text).DisposeWith(d);
            }
        );
    }

    #endregion Constructors

    #region Methods

    private void OnAliasSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Aliases.ScrollIntoView(Aliases.SelectedItem);
        ViewModel?.HydrateSelectedAlias();
    }

    private void OnClickLuaEditor(object sender, RoutedEventArgs e) { ViewModel!.EditLuaScript.Execute(); }

    #endregion Methods
}