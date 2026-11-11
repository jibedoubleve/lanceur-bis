using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.ViewModels.Controls;

public partial class ChartFilterViewModel : ObservableObject
{
    #region Constructors

    public ChartFilterViewModel(ChartFilter filter)
    {
        ParamName = filter.Parameter;
        Options = filter.Options;
        Title = filter.Title;
    }

    #endregion

    #region Properties

    [ObservableProperty]
    public partial object CurrentOption { get; set; }

    public bool IsSearchable => Options.Count > 15;
    public IDictionary<string, object?> Options { get; }
    public string ParamName { get; }

    public string Title { get; }

    #endregion
}