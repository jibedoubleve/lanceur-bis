using ModernWpf.Controls;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : IViewFor<SettingsViewModel>
    {
        #region Constructors

        public SettingsView()
        {
            InitializeComponent();

            DataContext = ViewModel;
            ViewModel = Locator.Current.GetService<SettingsViewModel>();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Router, v => v.RoutedViewHost.Router).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.Title, v => v.NavigationView.Header).DisposeWith(d);
            });
        }

        #endregion Constructors

        #region Methods

        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var tag = (string)((NavigationViewItem)args.SelectedItem).Tag;
            ViewModel.PushNavigation.Execute(tag).Subscribe();
        }

        #endregion Methods
    }
}