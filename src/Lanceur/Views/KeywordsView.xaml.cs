using Lanceur.Ui;
using Lanceur.Utils;
using ModernWpf.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for KeywordsView.xaml
    /// </summary>
    public partial class KeywordsView : IViewFor<KeywordsViewModel>
    {
        #region Constructors

        public KeywordsView()
        {
            InitializeComponent();
            DataContext = ViewModel;

            this.WhenActivated(d =>
            {
                LogService.Current.Trace($"Activating {nameof(KeywordsView)}");

                ViewModel.ConfirmRemove.RegisterHandler(async interaction =>
                {
                    var result = await Dialogs.YesNoQuestion($"Do you want to delete alias '{interaction.Input}'?");
                    interaction.SetOutput(result.AsBool());
                });


                this.OneWayBind(ViewModel, vm => vm.Aliases, v => v.Aliases.ItemsSource).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.BusyMessage, v => v.BusyMessage.Text).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.BusyControl.Visibility).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.IsBusy, v => v.AliasList.Visibility, val => val ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.SearchQuery, v => v.QueryBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedAlias, v => v.Aliases.SelectedItem).DisposeWith(d);

                //Issue 77: Duplicate keyword does not work. Should be fixed
                //this.BindCommand(ViewModel, vm => vm.DuplicateAlias, v => v.MenuDuplicate).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.CreateAlias, v => v.BtnCreateAlias).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveAlias, v => v.BtnDeleteAlias, v => v.SelectedAlias).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveOrUpdateAlias, v => v.BtnSaveOrUpdateAlias, v => v.SelectedAlias).DisposeWith(d);

                this.BindValidation(ViewModel, v => v.BoxFileNameValidation.Text).DisposeWith(d);
            });
        }

        #endregion Constructors

        #region Methods

        private void OnAliasSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Aliases.ScrollIntoView(Aliases.SelectedItem);
        }

        #endregion Methods
    }
}