using Lanceur.Ui;
using Lanceur.Utils;
using ModernWpf.Controls;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;

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
                Disposable
                    .Create(() => LogService.Current.Trace($"Deactivating {nameof(KeywordsView)}"))
                    .DisposeWith(d);


                ViewModel.ConfirmRemove.RegisterHandler(async interaction =>
                {
                    var result = await Dialogs.YesNoQuestion($"Do you want to delete alias '{interaction.Input}'?");
                    interaction.SetOutput(result.AsBool());
                });

                this.OneWayBind(ViewModel, vm => vm.Aliases, v => v.Aliases.ItemsSource).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SearchQuery, v => v.QueryBox.Text).DisposeWith(d);
                this.Bind(ViewModel, vm => vm.SelectedAlias, v => v.Aliases.SelectedItem).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.DuplicateAlias, v => v.MenuDuplicate).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.CreateAlias, v => v.BtnCreateAlias).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveAlias, v => v.BtnDeleteAlias, v => v.SelectedAlias).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.SaveOrUpdateAlias, v => v.BtnSaveOrUpdateAlias, v => v.SelectedAlias).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
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