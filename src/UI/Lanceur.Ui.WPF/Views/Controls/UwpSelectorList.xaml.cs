using Lanceur.Core.Models;

namespace Lanceur.Ui.WPF.Views.Controls;

public partial class UwpSelectorList
{
    #region Constructors

    public UwpSelectorList(UwpSelector vm)
    {
        DataContext = vm;
        InitializeComponent();
    }

    #endregion
}