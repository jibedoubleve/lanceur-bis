using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Repositories.Config;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Lanceur.Ui.Core.Constants;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public enum ReportType
{
    None,
    DoubloonAliases,
    BrokenAliases,
    UnannotatedAliases,
    RestoreAlias,
    UnusedAliases,
    InactiveAliases,
    RarelyUsedAliases
}

public partial class DataReconciliationViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<SelectableAliasQueryResult> _aliases = new();
    private ObservableCollection<SelectableAliasQueryResult> _buffer = new();
    private readonly ILogger<DataReconciliationViewModel> _logger;
    private readonly IReconciliationService _reconciliationService;
    [ObservableProperty] private ReportType _reportType = ReportType.None;
    private readonly IAliasRepository _repository;
    private readonly ISettingsFacade _settingsFacade;
    [ObservableProperty] private string _title = string.Empty;
    private readonly IUserInteractionService _userInteraction;
    private readonly IUserNotificationService _userNotification;
    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public DataReconciliationViewModel(
        IAliasRepository repository,
        ILogger<DataReconciliationViewModel> logger,
        IUserInteractionService userInteraction,
        IUserNotificationService userNotification,
        IReconciliationService reconciliationService,
        IViewFactory viewFactory,
        ISettingsFacade settingsFacade
    )
    {
        _repository = repository;
        _logger = logger;
        _userInteraction = userInteraction;
        _userNotification = userNotification;
        _reconciliationService = reconciliationService;
        _viewFactory = viewFactory;
        _settingsFacade = settingsFacade;
    }

    #endregion

    #region Methods

    private bool CanMerge()
    {
        if (!HasSelection()) return false;

        return GetSelectedAliases()
               .Select(e => e.FileName)
               .Distinct()
               .Count() ==
               1;
    }

    private SelectableAliasQueryResult[] GetSelectedAliases() => Aliases.Where(e => e.IsSelected).ToArray();

    private async Task<(bool IsSuccess, int NumericValue)> GetThreshold(string label, string toolTip, int minimum, int maximum, int numericValue)
    {
        var vm = new NumericSelectorViewModel
        {
            Label = label,
            ToolTip = toolTip,
            Minimum = minimum,
            Maximum = maximum,
            NumericValue = numericValue
        };

        var answer = await _userInteraction.AskUserYesNoAsync(
            _viewFactory.CreateView(vm),
            ButtonLabels.Ok,
            ButtonLabels.Cancel,
            "Threshold"
        );
        return answer
            ? (true, (int)vm.NumericValue)
            : (false, 0);
    }

    private bool HasSelection() => Aliases.Any(e => e.IsSelected);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnDelete()
    {
        var toDelete = GetSelectedAliases();
        var response = await _userInteraction.AskUserYesNoAsync($"Do you want to delete the {toDelete.Length} selected aliases?");
        if (!response) return;

        await Task.Run(() => _repository.RemoveLogically(toDelete));
        Aliases.RemoveMultiple(toDelete);
        _userNotification.Success($"Deleted {toDelete.Length} aliases.");
        _logger.LogInformation("Deleted {Items} aliases", toDelete.Length);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnDeletePermanently()
    {
        var toDelete = GetSelectedAliases();
        var response = await _userInteraction.AskUserYesNoAsync(
            $"""
             Are you sure you want to permanently delete the {toDelete.Length} selected aliases? 

             This action is irreversible.
             """
        );
        if (!response) return;

        await Task.Run(() => _repository.RemovePermanently(toDelete));
        Aliases.RemoveMultiple(toDelete);
        _userNotification.Success($"Permanently deleted {toDelete.Length} aliases.");
        _logger.LogInformation("Permanently deleted {Items} aliases", toDelete.Length);
    }

    [RelayCommand]
    private async Task OnFilterAlias(string filter)
    {
        IEnumerable<SelectableAliasQueryResult> results;
        if (string.IsNullOrWhiteSpace(filter))
        {
            Aliases = new(_buffer);
            return;
        }

        results = await Task.Run(
            () => _buffer.Where(e => e.Name.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase))
        );
        Aliases = new(results);
    }

    [RelayCommand]
    private void OnMarkSameIdAsSelected(AliasQueryResult alias)
    {
        if (alias == null) throw new ArgumentNullException(nameof(alias), "No alias was selected, did you forget to setup CommandParameter?");

        var selected = Aliases.Where(e => e.Id == alias.Id);
        foreach (var item in selected) item.IsSelected = true;
    }

    [RelayCommand(CanExecute = nameof(CanMerge))]
    private async Task OnMerge(object content)
    {
        using var _ = _logger.BeginCorrelatedLogs();

        var selectedAliases = GetSelectedAliases().ToList();
        if (selectedAliases.Count == 0) return;


        var alias = await Task.Run(
            () => _repository.GetById(
                selectedAliases.FirstOrDefault()!.Id
            )
        );
        _repository.MergeHistory(selectedAliases.Select(e => e.Id), alias.Id);
        var parameters = _repository.GetAdditionalParameter(
                                        selectedAliases.Select(item => item.Id)
                                    )
                                    .ToList();

        alias.AdditionalParameters = new(parameters);
        alias.AddDistinctSynonyms(selectedAliases.Select(e => e.Name));

        var dataContext = new DoubloonViewModel(parameters, alias.Synonyms);
        var response = await _userInteraction.InteractAsync(
            content,
            "Update changes",
            ButtonLabels.Cancel,
            "Merge aliases",
            dataContext
        );
        if (!response.IsConfirmed) return;

        alias.Synonyms = dataContext.Synonyms;

        await Task.Run(
            () =>
            {
                _repository.SaveOrUpdate(ref alias);
                _repository.Restore(alias);
            }
        );

        // Removing merged aliases
        var toRemove = selectedAliases.Where(e => e.Id != alias.Id);
        await Task.Run(() => _repository.Remove(toRemove));

        //Reload when finished
        await OnShowDoubloons();

        _userNotification.Success($"Aliases merged into alias '{alias.Name}'.");
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnRestore()
    {
        var selectedAliases = GetSelectedAliases();

        var response = await _userInteraction.AskUserYesNoAsync($"Do you want to restore {selectedAliases.Length} selected aliases?");
        if (!response) return;

        _repository.Restore(selectedAliases);
        _userNotification.Success($"Restored {selectedAliases.Length} selected aliases");
        _logger.LogInformation("Restored {Items} aliases", selectedAliases.Length);
        await OnShowRestoreAliases();
    }

    [RelayCommand] private void OnSelectionChanged() => this.NotifyCommandsUpdate();

    [RelayCommand]
    private async Task OnSetInactivityThreshold()
    {
        var result = await GetThreshold(
            "Select Inactivity Period (in months)",
            "The inactivity period refers to the number of months after which an alias is considered inactive.",
            0,
            12 * 30,
            _settingsFacade.Application.Reconciliation.InactivityThreshold
        );

        if (!result.IsSuccess) return;

        if (_settingsFacade.Application.Reconciliation.InactivityThreshold != result.NumericValue)
        {
            _settingsFacade.Application.Reconciliation.InactivityThreshold = result.NumericValue;
            _settingsFacade.Save();
            _logger.LogTrace("Inactivity threshold updated to {Months} months", result.NumericValue);
        }

        await OnShowInactiveAliases();
    }

    [RelayCommand]
    private async Task OnSetLowUsageThreshold()
    {
        var result = await GetThreshold(
            "Select Maximum Usage Count",
            "The usage threshold refers to the number of accesses below which an alias is considered to have low usage.",
            0,
            int.MaxValue,
            _settingsFacade.Application.Reconciliation.LowUsageThreshold
        );

        if (!result.IsSuccess) return;

        if (_settingsFacade.Application.Reconciliation.LowUsageThreshold != result.NumericValue)
        {
            _settingsFacade.Application.Reconciliation.LowUsageThreshold = result.NumericValue;
            _settingsFacade.Save();
            _logger.LogTrace("Low Usage threshold updated to {Count}", result.NumericValue);
        }

        await OnShowRarelyUsedAliases();
    }

    [RelayCommand]
    private async Task OnShowAliasesWithoutNotes() => await ShowAsync(
        "Show aliases without comments",
        ReportType.UnannotatedAliases,
        _repository.GetAliasesWithoutNotes,
        true
    );

    [RelayCommand] private async Task OnShowBrokenAliases() => await ShowAsync("Broken Aliases", ReportType.BrokenAliases, _repository.GetBrokenAliases);

    [RelayCommand] private async Task OnShowDoubloons() => await ShowAsync("Doubloon Aliases", ReportType.DoubloonAliases, _repository.GetDoubloons);

    [RelayCommand]
    private async Task OnShowInactiveAliases() => await ShowAsync(
        "Show inactive aliases",
        ReportType.InactiveAliases,
        () =>  _repository.GetInactiveAliases(_settingsFacade.Application.Reconciliation.InactivityThreshold)
    );

    [RelayCommand]
    private async Task OnShowRarelyUsedAliases() => await ShowAsync(
        "Show rarely used aliases",
        ReportType.RarelyUsedAliases,
        () =>  _repository.GetRarelyUsedAliases(_settingsFacade.Application.Reconciliation.LowUsageThreshold)
    );

    [RelayCommand] private async Task OnShowRestoreAliases() => await ShowAsync("Show deleted aliases", ReportType.RestoreAlias, _repository.GetDeletedAlias);

    [RelayCommand] private async Task OnShowUnusedAliases() => await ShowAsync("Unused Aliases", ReportType.UnusedAliases, _repository.GetUnusedAliases);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnUpdateDescription()
    {
        var selectedAliases = GetSelectedAliases();

        var response = await _userInteraction.AskUserYesNoAsync($"Do you want to update the description the {selectedAliases.Length} selected aliases?");
        if (!response) return;

        _repository.SaveOrUpdate(selectedAliases);
        _userNotification.Success($"Updated {selectedAliases.Length} selected aliases");
        _logger.LogInformation("Updated {Items} aliases", selectedAliases.Length);
        await OnShowAliasesWithoutNotes();
    }

    private async Task ShowAsync(string title, ReportType reportType, Func<IEnumerable<SelectableAliasQueryResult>> refreshAliases, bool isDescriptionUpdated = false)
    {
        using var loading = _userNotification.TrackLoadingState();
        Title = title;
        ReportType = reportType;

        var aliases = await Task.Run(refreshAliases);
        _buffer = new(aliases);
        Aliases = _buffer;

        if (isDescriptionUpdated) _ = _reconciliationService.ProposeDescriptionAsync(Aliases); // Fire & forget
        OnSelectionChanged();
    }

    #endregion
}