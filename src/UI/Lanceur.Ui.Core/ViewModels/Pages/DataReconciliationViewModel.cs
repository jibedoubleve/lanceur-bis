using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.Models;
using Lanceur.Core.Repositories;
using Lanceur.Core.Services;
using Lanceur.Infra.Logging;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public enum ReportType
{
    None,
    DoubloonAliases,
    BrokenAliases,
    UnannotatedAliases,
    RestoreAlias
}

public partial class DataReconciliationViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<SelectableAliasQueryResult> _aliases = new();
    private readonly ILogger<DataReconciliationViewModel> _logger;
    private readonly IReconciliationService _reconciliationService;
    [ObservableProperty] private ReportType _reportType = ReportType.None;
    private readonly IAliasRepository _repository;
    [ObservableProperty] private string _title = string.Empty;
    private readonly IUserInteractionService _userInteraction;
    private readonly IUserNotificationService _userNotification;

    #endregion

    #region Constructors

    public DataReconciliationViewModel(
        IAliasRepository repository,
        ILogger<DataReconciliationViewModel> logger,
        IUserInteractionService userInteraction,
        IUserNotificationService userNotification,
        IReconciliationService reconciliationService
    )
    {
        _repository = repository;
        _logger = logger;
        _userInteraction = userInteraction;
        _userNotification = userNotification;
        _reconciliationService = reconciliationService;
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

    private bool HasSelection() => Aliases.Any(e => e.IsSelected);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task OnDelete()
    {
        var toDelete = GetSelectedAliases();
        var response = await _userInteraction.AskUserYesNoAsync($"Do you want to delete the {toDelete.Length} selected aliases?");
        if (!response) return;

        await Task.Run(() => _repository.Remove(toDelete));
        Aliases.RemoveMultiple(toDelete);
        _userNotification.Success($"Deleted {toDelete.Length} aliases.");
        _logger.LogInformation("Deleted {Items} aliases", toDelete.Length);
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

        var selectedAliases = GetSelectedAliases().ToArray();
        if (selectedAliases.Length == 0) return;

        var firstSelectedAlias = selectedAliases.FirstOrDefault();
        var parameters = selectedAliases.Where(e => !e.Parameters.IsNullOrEmpty())
                                        .Select(item => new AdditionalParameter { Name = item.Name, Parameter = item.Parameters })
                                        .ToList();

        var alias = await Task.Run(() => _repository.GetByIdAndName(firstSelectedAlias!.Id));
        alias.AddDistinctSynonyms(selectedAliases.Select(e => e.Name));
        alias.AdditionalParameters(_repository.GetAdditionalParameter(selectedAliases.Select(e => e.Id).ToArray()));

        var dataContext = new DoubloonViewModel(parameters, alias.Synonyms);

        var response = await _userInteraction.AskUserYesNoAsync(
            content,
            "Update changes",
            "Cancel",
            "Merge aliases",
            dataContext
        );
        if (!response) return;

        // Merge all the alias into this one (That's add additional parameters) 
        alias.Synonyms = dataContext.Synonyms;
        foreach (var item in dataContext.List) alias.AdditionalParameters.Add(new() { Name = item.Name, Parameter = item.Parameter });

        await Task.Run(() => _repository.SaveOrUpdate(ref alias));

        // Removing merged aliases
        _repository.Remove(selectedAliases.Where(e => e.Id != alias.Id).Select(a => a));

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
        await OnShowRestoreAlias();
    }

    [RelayCommand]
    private void OnSelectionChanged()
    {
        DeleteCommand.NotifyCanExecuteChanged();
        MergeCommand.NotifyCanExecuteChanged();
        RestoreCommand.NotifyCanExecuteChanged();
        UpdateDescriptionCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task OnShowAliasesWithoutNotes() => await ShowAsync(
        "Show aliases without comments",
        ReportType.UnannotatedAliases,
        _repository.GetAliasesWithoutNotes,
        true
    );

    [RelayCommand] private async Task OnShowDoubloons() => await ShowAsync("Doubloon Aliases", ReportType.DoubloonAliases, _repository.GetDoubloons);

    [RelayCommand] private async Task OnShowBrokenAliases() => await ShowAsync("Broken Aliases", ReportType.BrokenAliases, _repository.GetBrokenAliases);

    [RelayCommand] private async Task OnShowRestoreAlias() => await ShowAsync("Show deleted aliases", ReportType.RestoreAlias, _repository.GetDeletedAlias);

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
        Aliases = new(aliases);

        if (isDescriptionUpdated) _ = _reconciliationService.ProposeDescriptionAsync(Aliases); // Fire & forget
        OnSelectionChanged();
    }

    #endregion
}