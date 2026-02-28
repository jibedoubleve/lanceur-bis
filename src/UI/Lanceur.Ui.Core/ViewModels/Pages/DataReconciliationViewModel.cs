using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Constants;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Lanceur.Ui.Core.Constants;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class DataReconciliationViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<SelectableAliasQueryResult> _aliases = new();
    private ObservableCollection<SelectableAliasQueryResult> _buffer = new();
    [ObservableProperty] private ReportConfiguration _currentReportConfiguration;
    private readonly ILogger<DataReconciliationViewModel> _logger;
    private readonly IReconciliationService _reconciliationService;
    [ObservableProperty] private ReportType _reportType = ReportType.None;
    private readonly IAliasRepository _repository;
    private readonly IWriteableSection<ReconciliationSection> _settingsFacade;
    private readonly IThumbnailService _thumbnailService;
    [ObservableProperty] private string _title = string.Empty;
    private DateTime? _today;
    private readonly IUserDialogueService _userDialogue;
    private readonly IUserNotificationService _userNotification;
    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public DataReconciliationViewModel(
        IAliasRepository repository,
        ILogger<DataReconciliationViewModel> logger,
        IUserCommunicationService interactions,
        IReconciliationService reconciliationService,
        IViewFactory viewFactory,
        IWriteableSection<ReconciliationSection> settingsFacade,
        IThumbnailService thumbnailService
    )
    {
        _repository = repository;
        _logger = logger;
        _userDialogue = interactions.Dialogues;
        _userNotification = interactions.Notifications;
        _reconciliationService = reconciliationService;
        _viewFactory = viewFactory;
        _settingsFacade = settingsFacade;
        _thumbnailService = thumbnailService;
        _currentReportConfiguration =
            _settingsFacade.Value.ReportsConfiguration
                           .FirstOrDefault(e => e.ReportType == ReportType.RestoreAlias)!;
    }

    #endregion

    #region Properties

    private ReconciliationSection Reconciliation => _settingsFacade.Value;

    #endregion

    #region Methods

    private bool CanMerge()
    {
        if (!HasSelection()) { return false; }

        return GetSelectedAliases()
               .Select(e => e.FileName)
               .Distinct()
               .Count() ==
               1;
    }


    private async Task<(bool IsSuccess, IEnumerable<ReportConfiguration>? Configuration)> GetReportConfiguration(
        string label,
        string tooltip,
        IEnumerable<ReportConfiguration> configuration,
        ReportType reportType = ReportType.None
    )
    {
        var vm = new ReportConfigurationViewModel(
            configuration,
            label,
            tooltip,
            reportType
        );
        var answer = await _userDialogue.AskUserYesNoAsync(
            _viewFactory.CreateView(vm),
            ButtonLabels.Ok,
            ButtonLabels.Cancel,
            "Configure reports"
        );
        return answer
            ? (answer, vm.Configurations)
            : (answer, null);
    }

    private SelectableAliasQueryResult[] GetSelectedAliases() => Aliases.Where(e => e.IsSelected).ToArray();

    private async Task<(bool IsSuccess, int NumericValue)> GetThreshold(
        string label,
        string toolTip,
        int minimum,
        int maximum,
        int numericValue
    )
    {
        var vm = new NumericSelectorViewModel
        {
            Label = label,
            ToolTip = toolTip,
            Minimum = minimum,
            Maximum = maximum,
            NumericValue = numericValue
        };

        var answer = await _userDialogue.AskUserYesNoAsync(
            _viewFactory.CreateView(vm),
            ButtonLabels.Ok,
            ButtonLabels.Cancel,
            "Threshold"
        );
        return answer
            ? (answer, (int)vm.NumericValue)
            : (answer, 0);
    }

    private bool HasSelection() => Aliases.Any(e => e.IsSelected);

    [RelayCommand]
    private async Task OnConfigureReport()
    {
        var result = await GetReportConfiguration(
            "Reports",
            "Configure the visibility of the columns in the report.",
            Reconciliation.ReportsConfiguration,
            ReportType
        );

        if (!result.IsSuccess) { return; }

        _settingsFacade.Save();
        await OnShowAsync(ReportType);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnDelete()
    {
        var toDelete = GetSelectedAliases();
        var response
            = await _userDialogue.AskUserYesNoAsync(
                $"Do you want to delete the {toDelete.Length} selected aliases?"
            );
        if (!response) { return; }

        await Task.Run(() => _repository.RemoveLogically(toDelete));
        Aliases.RemoveMultiple(toDelete);
        _userNotification.Success($"Deleted {toDelete.Length} aliases.");
        _logger.LogInformation("Deleted {Items} aliases", toDelete.Length);
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnDeletePermanently()
    {
        var toDelete = GetSelectedAliases();
        var response = await _userDialogue.AskUserYesNoAsync(
            $"""
             Are you sure you want to permanently delete the {toDelete.Length} selected aliases? 

             This action is irreversible.
             """
        );
        if (!response) { return; }

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

        results = await Task.Run(() => _buffer.Where(e => e.Name.StartsWith(
                    filter,
                    StringComparison.CurrentCultureIgnoreCase
                )
            )
        );
        Aliases = new(results);
    }

    [RelayCommand]
    private void OnLoadThumbnail(QueryResult? queryResult)
    {
        if (queryResult is null) { return; }

        if (!queryResult.Thumbnail.IsNullOrEmpty()) { return; /* Already loaded */ }

        try { _thumbnailService.UpdateThumbnail(queryResult); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load thumbnail for alias id {IdAlias}", queryResult.Id);
        }
    }

    [RelayCommand]
    private void OnMarkSameIdAsSelected(AliasQueryResult alias)
    {
        if (alias == null)
        {
            throw new ArgumentNullException(
                nameof(alias),
                "No alias was selected, did you forget to setup CommandParameter?"
            );
        }

        var selected = Aliases.Where(e => e.Id == alias.Id);
        foreach (var item in selected) item.IsSelected = true;
    }

    [RelayCommand(CanExecute = nameof(CanMerge))]
    private async Task OnMerge(object content)
    {
        using var _ = _logger.BeginCorrelatedLogs();

        var selectedAliases = GetSelectedAliases().ToList();
        if (selectedAliases.Count == 0) { return; }


        var (alias, parameters) = await Task.Run(() =>
            {
                var alias = _repository.GetById(selectedAliases.FirstOrDefault()!.Id);

                _repository.MergeHistory(selectedAliases.Select(e => e.Id), alias.Id);

                var parameters = _repository.GetAdditionalParameter(
                                                selectedAliases.Select(item => item.Id)
                                            )
                                            .ToList();

                return (alias, parameters);
            }
        );

        /* To be thread safe, this code should NOT be in the upper Task. If AdditionalParameters and AddDistinctSynonyms
         * raise a PropertyChanged event it'll be from the thread and will handled in the UI thread. This could lead to
         * a cross-thread exception...
         */
        alias.AdditionalParameters = new(parameters);
        alias.AddDistinctSynonyms(selectedAliases.Select(e => e.Name));

        var dataContext = new DoubloonViewModel(parameters, alias.Synonyms);
        var response = await _userDialogue.InteractAsync(
            content,
            "Update changes",
            ButtonLabels.Cancel,
            "Merge aliases",
            dataContext
        );
        if (!response.IsConfirmed) { return; }

        alias.Synonyms = dataContext.Synonyms;

        await Task.Run(() =>
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

        var response
            = await _userDialogue.AskUserYesNoAsync(
                $"Do you want to restore {selectedAliases.Length} selected aliases?"
            );
        if (!response) { return; }

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
            Reconciliation.InactivityThreshold
        );

        if (!result.IsSuccess) { return; }

        if (Reconciliation.InactivityThreshold != result.NumericValue)
        {
            Reconciliation.InactivityThreshold = result.NumericValue;
            _settingsFacade.Save();
            _logger.LogDebug("Inactivity threshold updated to {Months} months", result.NumericValue);
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
            Reconciliation.LowUsageThreshold
        );

        if (!result.IsSuccess) { return; }

        if (Reconciliation.LowUsageThreshold != result.NumericValue)
        {
            Reconciliation.LowUsageThreshold = result.NumericValue;
            _settingsFacade.Save();
            _logger.LogDebug("Low Usage threshold updated to {Count}", result.NumericValue);
        }

        await OnShowRarelyUsedAliases();
    }


    [RelayCommand]
    private async Task OnShowAliasesWithoutNotes() => await OnShowAsync(ReportType.UnannotatedAliases, true);

    private async Task OnShowAsync(ReportType reportType, bool isDescriptionUpdated = false)
    {
        using var loading = _userNotification.TrackLoadingState();
        Title = reportType switch
        {
            ReportType.DoubloonAliases    => "Doubloon Aliases",
            ReportType.BrokenAliases      => "Broken Aliases",
            ReportType.UnannotatedAliases => "Show aliases without comments",
            ReportType.RestoreAlias       => "Show deleted aliases",
            ReportType.UnusedAliases      => "Unused Aliases",
            ReportType.InactiveAliases    => "Show inactive aliases",
            ReportType.RarelyUsedAliases  => "Show rarely used aliases",
            ReportType.None               => "No report selected",
            _                             => throw new ArgumentOutOfRangeException($"Report '{reportType}' not found")
        };

        Func<IEnumerable<SelectableAliasQueryResult>> refreshAliases = reportType switch
        {
            ReportType.DoubloonAliases    => _repository.GetDoubloons,
            ReportType.BrokenAliases      => _repository.GetBrokenAliases,
            ReportType.UnannotatedAliases => _repository.GetAliasesWithoutNotes,
            ReportType.RestoreAlias       => _repository.GetDeletedAlias,
            ReportType.UnusedAliases      => _repository.GetUnusedAliases,
            ReportType.InactiveAliases    => ()
                => _repository.GetInactiveAliases(Reconciliation.InactivityThreshold, _today),
            ReportType.RarelyUsedAliases => () => _repository.GetRarelyUsedAliases(Reconciliation.LowUsageThreshold),
            _                            => throw new ArgumentOutOfRangeException($"Report '{reportType}' not found")
        };

        ReportType = reportType;
        CurrentReportConfiguration
            = Reconciliation.ReportsConfiguration.FirstOrDefault(e => e.ReportType == reportType)!;

        var aliases = await Task.Run(refreshAliases);
        _buffer = new(aliases);
        Aliases = _buffer;

        if (isDescriptionUpdated)
        {
            _ = _reconciliationService.ProposeDescriptionAsync(Aliases); // Fire & forget
        }

        OnSelectionChanged();
    }

    [RelayCommand] private async Task OnShowBrokenAliases() => await OnShowAsync(ReportType.BrokenAliases);

    [RelayCommand] private async Task OnShowDoubloons() => await OnShowAsync(ReportType.DoubloonAliases);

    [RelayCommand] private async Task OnShowInactiveAliases() => await OnShowAsync(ReportType.InactiveAliases);

    [RelayCommand] private async Task OnShowRarelyUsedAliases() => await OnShowAsync(ReportType.RarelyUsedAliases);

    [RelayCommand] private async Task OnShowRestoreAliases() => await OnShowAsync(ReportType.RestoreAlias);

    [RelayCommand] private async Task OnShowUnusedAliases() => await OnShowAsync(ReportType.UnusedAliases);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnUpdateDescription()
    {
        var selectedAliases = GetSelectedAliases();

        var response = await _userDialogue.AskUserYesNoAsync(
            $"Do you want to update the description the {selectedAliases.Length} selected aliases?"
        );
        if (!response) { return; }

        _repository.SaveOrUpdate(selectedAliases);
        _userNotification.Success($"Updated {selectedAliases.Length} selected aliases");
        _logger.LogInformation("Updated {Items} aliases", selectedAliases.Length);
        await OnShowAliasesWithoutNotes();
    }

    /// <summary>
    ///     Overrides the current date used by the ViewModel.
    ///     Intended for testing scenarios to simulate different "today" values
    ///     and verify inactivity chart behaviour.
    /// </summary>
    /// <param name="today">
    ///     Optional custom date to use as the current day.
    ///     If null, restores the default system date.
    /// </param>
    public void OverrideToday(DateTime? today = null) => _today = today;

    #endregion
}