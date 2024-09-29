using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.Managers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Services;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class KeywordsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<AliasQueryResult> _aliases = new();
    private readonly IAliasManagementService _aliasManagementService;
    private IList<AliasQueryResult> _bufferedAliases = Array.Empty<AliasQueryResult>();
    private readonly ILogger<KeywordsViewModel> _logger;
    private readonly INotificationService _notificationService;
    private readonly IAliasValidationService _validationService;
    private AliasQueryResult? _selectedAlias;
    private readonly IThumbnailManager _thumbnailManager;

    #endregion

    #region Constructors

    public KeywordsViewModel(
        IAliasManagementService aliasManagementService,
        IThumbnailManager thumbnailManager,
        ILogger<KeywordsViewModel> logger,
        INotificationService notificationService,
        IAliasValidationService validationService
    )
    {
        ArgumentNullException.ThrowIfNull(aliasManagementService);
        ArgumentNullException.ThrowIfNull(notificationService);
        ArgumentNullException.ThrowIfNull(thumbnailManager);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(validationService);

        _notificationService = notificationService;
        _validationService = validationService;
        _aliasManagementService = aliasManagementService;
        _thumbnailManager = thumbnailManager;
        _logger = logger;
    }

    #endregion

    #region Properties

    public AliasQueryResult? SelectedAlias
    {
        get => _selectedAlias;
        set
        {
            SetProperty(ref _selectedAlias, value);
            SaveCurrentAliasCommand.NotifyCanExecuteChanged();
            DeleteCurrentAliasCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion

    #region Methods

    private bool CanExecuteCurrentAlias() => SelectedAlias != null;

    [RelayCommand]
    private void OnCreateAlias()
    {
        if (Aliases.Any(x => x.Id == 0))
        {
            SelectedAlias = Aliases.Single(x => x.Id == 0);
            _logger.LogTrace("Reselect alias to be created.");
            return;
        }

        _logger.LogTrace("Creating new alias");
        Aliases.Insert(0, AliasQueryResult.EmptyForCreation);
        SelectedAlias = Aliases[0];
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCurrentAlias))]
    private async Task OnDeleteCurrentAliasAsync()
    {
        var aliasName = SelectedAlias!.Name;
        if (SelectedAlias.Id == 0)
        {
            Aliases.Remove(SelectedAlias);
            _notificationService.Success("Cancel creation.", $"Abort creation of alias {aliasName}.");
            return;
        }

        _logger.LogTrace("Deleting alias {AliasName}", aliasName);
        var response = await WeakReferenceMessenger.Default.Send<Ask>(new("Do you want to delete '{0}'?".Format(SelectedAlias.Name)));

        if (!response) return;

        await Task.Run(() => _aliasManagementService.Delete(SelectedAlias));
        var toDelete = Aliases.Where(x => x.Id == SelectedAlias.Id).ToArray();
        foreach (var item in toDelete) Aliases.Remove(item);
        _notificationService.Success("Item deleted.", $"Alias {aliasName} deleted.");
    }

    [RelayCommand]
    private async Task OnLoadAliases()
    {
        if (_bufferedAliases.Count > 0) return;

        await Task.Delay(50);

        _bufferedAliases = await Task.Run(() => _aliasManagementService.GetAll()!.ToList());

        SelectedAlias = _bufferedAliases[0];
        Aliases = new(_bufferedAliases);
        _thumbnailManager.RefreshThumbnails(_bufferedAliases);
        _logger.LogTrace("Loaded {Count} alias(es)", _bufferedAliases.Count());
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCurrentAlias))]
    private async Task OnLoadCurrentAliasAsync()
    {
        SelectedAlias = await Task.Run(() => _aliasManagementService.Hydrate(SelectedAlias));
        _logger.LogTrace("Loading alias {AliasName}", SelectedAlias.Name);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCurrentAlias))]
    private async Task OnSaveCurrentAliasAsync()
    {
        var result = _validationService.IsValid(SelectedAlias);
        if (!result.IsSuccess)
        {
            _notificationService.Warn("Validation failed", result.ErrorContent);
            _logger.LogInformation("Validation failed for {AliasName}: {Errors}", SelectedAlias!.Name, result.ErrorContent);
            return;
        }

        _logger.LogTrace("Saving alias {AliasName}", SelectedAlias!.Name);
        var alias = SelectedAlias;
        await Task.Run(() => _aliasManagementService.SaveOrUpdate(ref alias));
        _notificationService.Success("Item created.", $"Alias {alias.Name} created.");
        await OnLoadAliases();
    }

    [RelayCommand]
    private void OnSearch(string criterion)
    {
        criterion = criterion.ToLower();
        var aliases = _bufferedAliases.Where(x => x.Name.ToLower().StartsWith(criterion))
                                      .ToArray();

        _logger.LogTrace("Found {Count} alias(es) with criterion {Criterion}", aliases.Length, criterion);
        Aliases = new(aliases);
    }

    #endregion
}