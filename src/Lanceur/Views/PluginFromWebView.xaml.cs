using ReactiveUI;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for PluginFromWebView.xaml
    /// </summary>
    public partial class PluginFromWebView : IViewFor<PluginFromWebViewModel>
    {
        #region Constructors

        public PluginFromWebView()
        {
            InitializeComponent();

            ViewModel = Locator.Current.GetService<PluginFromWebViewModel>();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PluginManifests, v => v.PluginManifests.ItemsSource).DisposeWith(d);

                this.Bind(ViewModel, vm => vm.SelectedManifest, v => v.PluginManifests.SelectedItem).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.SelectedManifest, v => v.BtnInstallSelected.IsEnabled, x => x is not null);

                ViewModel.Activate.Execute().Subscribe();
            });

            Closed += OnClosed;
        }

        #endregion Constructors

        #region Properties

        public InteractionContext<Unit, string> Interaction { get; internal set; }

        #endregion Properties

        #region Methods

        private void OnClickBtnInstallSelected(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectionValidated = true;
            Close();
        }

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectionValidated = false;
            Close();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            var output = ViewModel.SelectionValidated
                ? ViewModel.SelectedManifest.Url
                : null;

            Interaction.SetOutput(output);
        }

        #endregion Methods
    }
}