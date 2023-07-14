using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for PluginsView.xaml
    /// </summary>
    public partial class PluginsView : IViewFor<PluginsViewModel>
    {
        #region Constructors

        public PluginsView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.PluginConfigurations, v => v.PluginConfigurations.ItemsSource).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
            });

        }

        #endregion Constructors
    }
}