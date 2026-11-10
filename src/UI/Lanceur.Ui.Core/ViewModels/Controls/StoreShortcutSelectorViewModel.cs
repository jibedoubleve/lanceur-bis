using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.SharedKernel.IoC;

namespace Lanceur.Ui.Core.ViewModels.Controls;

/// <summary>
///     Lets the user pick a store shortcut among the special characters that are still free.
/// </summary>
[ExcludeFromIoC]
public partial class StoreShortcutSelectorViewModel : ObservableObject
{
    #region Constructors

    public StoreShortcutSelectorViewModel(IEnumerable<string> availableShortcuts, string? selectedShortcut)
    {
        ArgumentNullException.ThrowIfNull(availableShortcuts);

        AvailableShortcuts = availableShortcuts.ToArray();
        SelectedShortcut = selectedShortcut;
    }

    #endregion

    #region Properties

    public IReadOnlyList<string> AvailableShortcuts { get; }

    [ObservableProperty]
    public partial string? SelectedShortcut { get; set; }

    #endregion
}