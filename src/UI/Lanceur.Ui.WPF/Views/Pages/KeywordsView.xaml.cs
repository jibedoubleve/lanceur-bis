using System.Windows.Controls;
using Lanceur.Ui.Core.ViewModels.Pages;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class KeywordsView : Page
{
    public KeywordsView(KeywordsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent(); 
    }
}