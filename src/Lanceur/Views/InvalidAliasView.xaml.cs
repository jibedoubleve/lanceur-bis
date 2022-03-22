using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for InvalidAliasView.xaml
    /// </summary>
    public partial class InvalidAliasView : IViewFor<InvalidAliasViewModel>
    {
        public InvalidAliasView()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.InvalidAliases, v => v.InvalidAliases.ItemsSource).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.RemoveSelected, v => v.BtnRemoveSelected).DisposeWith(d);

                ViewModel.Activate.Execute().Subscribe();
            });
        }
    }
}
