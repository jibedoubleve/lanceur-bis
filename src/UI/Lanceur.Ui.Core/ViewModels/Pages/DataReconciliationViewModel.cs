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

public enum ReportType { None, DoubloonAliases, InvalidAliases }

public partial class DataReconciliationViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<SelectableAliasQueryResult> _aliases = new();
    private readonly ILogger<DataReconciliationViewModel> _logger;
    [ObservableProperty] private ReportType _reportType = ReportType.None;
    private readonly IDbRepository _repository;
    [ObservableProperty] private string _title = string.Empty;
    private readonly IUserInteractionService _userInteraction;
    private readonly IUserNotificationService _userNotification;

    #endregion

    #region Constructors

    public DataReconciliationViewModel(
        IDbRepository repository,
        ILogger<DataReconciliationViewModel> logger,
        IUserInteractionService userInteraction,
        IUserNotificationService userNotification
    )
    {
        _repository = repository;
        _logger = logger;
        _userInteraction = userInteraction;
        _userNotification = userNotification;
    }

    #endregion

    #region Methods

    private bool CanExecuteCommand() => Aliases.Any(e => e.IsSelected);

    private bool CanMerge()
    {
        if (!CanExecuteCommand()) return false;

        return GetSelectedAliases()
               .Select(e => e.FileName)
               .Distinct()
               .Count() == 1;
    }

    private SelectableAliasQueryResult[] GetSelectedAliases() => Aliases.Where(e => e.IsSelected).ToArray();

    [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
    private async Task OnDelete()
    {
        var toDelete = GetSelectedAliases();
        var response = await _userInteraction.AskUserYesNoAsync($"Do you want to delete the {toDelete.Length} selected aliases?");
        if (!response) return;

        await Task.Run(() => _repository.RemoveMany(toDelete));
        Aliases.RemoveMultiple(toDelete);
        _userNotification.Success($"Deleted {toDelete.Length} aliases.");
        _logger.LogInformation("Deleted {Items} aliases", toDelete.Length);
    }

    [RelayCommand(CanExecute = nameof(CanMerge))]
    private async Task OnMerge(object content)
    {
        using var _ = _logger.BeginCorrelatedLogs();

        var selectedAliases = GetSelectedAliases().ToArray();
        if (selectedAliases.Length == 0) return;
        
        var firstSelectedAlias = selectedAliases.FirstOrDefault();
        var parameters = selectedAliases.Where(e => !e.Parameters.IsNullOrEmpty())
                                        .Select(item => new KeyValueViewModel<string, string>(item.Name, item.Parameters))
                                        .ToList();
        
        var alias = await Task.Run(() => _repository.GetByIdAndName(firstSelectedAlias!.Id, firstSelectedAlias.Name));
        alias.AddDistinctSynonyms(selectedAliases.Select(e => e.Name));
        alias.AdditionalParameters(_repository.GetAdditionalParameter(selectedAliases.Select(e => e.Id).ToArray()));

        var dataContext = new KeyValueListViewModel<string, string>(parameters, alias.Synonyms);

        var response = await _userInteraction.AskUserYesNoAsync(content, "Update changes", "Cancel", "Merge aliases", dataContext);
        if (!response) return;

        // Merge all the alias into this one (That's add additional parameters) 
        alias.Synonyms = dataContext.Synonyms;
        foreach (var item in dataContext.List) alias.AdditionalParameters.Add(new() { Name = item.Key, Parameter = item.Value });

        await Task.Run(() => _repository.SaveOrUpdate(ref alias));

        // Removing merged aliases
        _repository.RemoveMany(selectedAliases.Where(e => e.Id != alias.Id).Select(a => a));

        //Reload when finished
        await OnShowDoubloons();

        _userNotification.Success($"Aliases merged into alias '{alias.Name}'.");
    }

    [RelayCommand]
    private void OnSelectionChanged()
    {
        DeleteCommand.NotifyCanExecuteChanged();
        MergeCommand?.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task OnShowDoubloons()
    {
        Title = "Doubloon Aliases";
        ReportType = ReportType.DoubloonAliases;
        var doubloons = await Task.Run(() => _repository.GetDoubloons());
        Aliases = new(doubloons);
        OnSelectionChanged();
    }

    [RelayCommand]
    private async Task OnShowEmptyKeywords()
    {
        Title = "Misconfigured Aliases";
        ReportType = ReportType.InvalidAliases;
        var doubloons = await Task.Run(() => _repository.GetInvalidAliases());
        Aliases = new(doubloons);
        OnSelectionChanged();
    }

    #endregion
}