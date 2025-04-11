using Lanceur.Core.Models;
using Lanceur.Core.Models.Settings;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Controls;
using Lanceur.Ui.Core.ViewModels.Pages;
using Lanceur.Ui.WPF.Views.Controls;

namespace Lanceur.Ui.WPF.Helpers;

public class ViewFactory : IViewFactory
{
    #region Methods

    /// <inheritdoc />
    public object CreateVersionView(string version, string commit) => new VersionView(version, commit);

    /// <inheritdoc />
    public object CreateView(object viewModel) => viewModel switch
    {
        null                                    => throw new ArgumentNullException(nameof(viewModel), "The view model cannot be null."),
        AdditionalParameter vm                  => new AdditionalParameterView(vm),
        StoreShortcut vm                        => new StoreShortcutControl(vm),
        MultipleAdditionalParameterViewModel vm => new MultipleAdditionalParameterView(vm),
        NumericSelectorViewModel vm             => new NumericSelectorControl(vm),
        _                                       => throw new NotSupportedException($"No control of type {viewModel.GetType().Name} is supported.")
    };

    #endregion
}