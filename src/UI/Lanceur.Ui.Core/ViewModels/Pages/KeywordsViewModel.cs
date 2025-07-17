using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Mappers;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Constants;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class KeywordsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<AliasQueryResult> _aliases = [];
    private readonly IAliasManagementService _aliasManagementService;
    private List<AliasQueryResult> _cachedAliases = [];
    [ObservableProperty] private string _criterion = string.Empty;
    private readonly IInteractionHubService _hubService;
    private readonly ILogger<KeywordsViewModel> _logger;
    private readonly IPackagedAppSearchService _packagedAppSearchService;
    private AliasQueryResult? _previousAlias;
    private AliasQueryResult? _selectedAlias;
    private readonly IThumbnailService _thumbnailService;
    private readonly IAliasValidationService _validationService;
    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public KeywordsViewModel(
        IAliasManagementService aliasManagementService,
        IThumbnailService thumbnailService,
        ILogger<KeywordsViewModel> logger,
        IInteractionHubService hubService,
        IAliasValidationService validationService,
        IViewFactory viewFactory,
        IPackagedAppSearchService packagedAppSearchService
    )
    {
        ArgumentNullException.ThrowIfNull(aliasManagementService);
        ArgumentNullException.ThrowIfNull(thumbnailService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(validationService);
        ArgumentNullException.ThrowIfNull(viewFactory);
        ArgumentNullException.ThrowIfNull(packagedAppSearchService);
        ArgumentNullException.ThrowIfNull(hubService);

        WeakReferenceMessenger.Default.Register<AddAliasMessage>(
            this,
            (r, m) => ((KeywordsViewModel)r).OnCreateAlias(m)
        );

        _validationService = validationService;
        _viewFactory = viewFactory;
        _packagedAppSearchService = packagedAppSearchService;
        _aliasManagementService = aliasManagementService;
        _thumbnailService = thumbnailService;
        _logger = logger;
        _hubService = hubService;
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
    private async Task OnAddMultiParameters()
    {
        var view = _viewFactory.CreateView(new MultipleAdditionalParameterViewModel());

        var result = await _hubService.Interactions.InteractAsync(
            view,
            ButtonLabels.Apply,
            ButtonLabels.Cancel,
            "Add multiple parameters"
        );
        if (!result.IsConfirmed) return;

        var viewModel = result.DataContext as MultipleAdditionalParameterViewModel;

        var parser = new TextToParameterParser();
        var parseResult = parser.Parse(viewModel!.RawParameters);

        if (!parseResult.Success)
        {
            _hubService.Notifications.Warning(
                "The parsing operation failed because the entered text is invalid and cannot be converted into parameters."
            );
            return;
        }

        _selectedAlias?.AdditionalParameters.AddRange(parseResult.Parameters);
    }

    [RelayCommand]
    private async Task OnAddParameter()
    {
        if (SelectedAlias is null) return;

        var parameter = this.NewAdditionalParameter();
        if (parameter is null)
        {
            _logger.LogInformation("No alias selected. Impossible to add new additional parameter");
            return;
        }

        var view = _viewFactory.CreateView(parameter);

        var result = await _hubService.Interactions.InteractAsync(
            view,
            ButtonLabels.Apply,
            ButtonLabels.Cancel,
            "Add parameter"
        );
        if (!result.IsConfirmed) return;

        var vm = result.DataContext as AdditionalParameter;
        SelectedAlias?.AdditionalParameters.Add(vm);
        _hubService.Notifications.Success(
            $"Parameter {parameter.Name} has been added. Don't forget to save to apply changes",
            "Updated."
        );
    }

    [RelayCommand]
    private void OnCreateAlias(AddAliasMessage? message = null)
    {
        if (Aliases.Any(x => x.Id == 0))
            // An alias for creation already exists in the list,
            // remove all these aliases...
            Aliases.RemoveWhere(e => e.Id == 0);

        var names = message?.Cmdline?.Parameters;
        var newAlias = names is null
            ? AliasQueryResult.EmptyForCreation
            : new() { Name = names, Synonyms = names };

        _logger.LogInformation("Creating new alias with name {Name}", names);
        Aliases.Insert(0, newAlias);
        SelectedAlias = Aliases[0];
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCurrentAlias))]
    private async Task OnDeleteCurrentAliasAsync()
    {
        var aliasName = SelectedAlias!.Name;
        if (SelectedAlias.Id == 0)
        {
            Aliases.Remove(SelectedAlias);
            _hubService.Notifications.Success($"Abort creation of alias {aliasName}.", "Cancel creation.");
            return;
        }

        var response = await _hubService.Interactions.AskUserYesNoAsync($"Do you want to delete {SelectedAlias.Name}?");
        if (!response) return;

        _logger.LogInformation("Deleting alias {AliasName}", aliasName);

        // Delete from DB
        await Task.Run(() => _aliasManagementService.Delete(SelectedAlias));

        // Delete from UI
        var toDelete = Aliases.Where(x => x.Id == SelectedAlias.Id).ToArray();

        Criterion = string.Empty;
        foreach (var item in toDelete) _cachedAliases.Remove(item);
        Aliases = new(_cachedAliases);
        SelectedAlias = Aliases.FirstOrDefault();

        _hubService.Notifications.Success($"Alias {aliasName} deleted.", "Item deleted.");
    }

    [RelayCommand]
    private async Task OnDeleteParameter(AdditionalParameter parameter)
    {
        var response = await _hubService.Interactions.AskUserYesNoAsync(
            $"The parameter '{parameter.Name}' will disappear from the screen and be permanently deleted only after you save your changes. Do you want to continue?"
        );

        if (!response) return;

        var toRemove = SelectedAlias?.AdditionalParameters
                                    .SingleOrDefault(x => x.Id == parameter.Id);
        SelectedAlias?.AdditionalParameters.Remove(toRemove);
    }

    [RelayCommand]
    private async Task OnEditParameter(AdditionalParameter parameter)
    {
        var view = _viewFactory.CreateView(parameter);

        var result = await _hubService.Interactions.AskUserYesNoAsync(
            view,
            ButtonLabels.Apply,
            ButtonLabels.Cancel,
            "Edit parameter"
        );
        if (!result) return;

        var param = SelectedAlias?.AdditionalParameters?.SingleOrDefault(x => x.Id == parameter.Id);
        if (param is null)
        {
            _logger.LogWarning("Parameter to update is not found in current alias.");
            return;
        }

        _hubService.Notifications.Success(
            $"Modification has been done on {parameter.Name}. Don't forget to save to apply changes",
            "Updated."
        );
    }

    [RelayCommand]
    private async Task OnLoadAliases()
    {
        var selectedAlias = SelectedAlias; // Keep a ref alive
        var result = await Task.Run(() => _aliasManagementService.GetAll());
        _cachedAliases = result.ToList();

        var newAlias = Aliases.FirstOrDefault(e => e.Id == 0);
        Aliases.Clear();

        if (newAlias is not null) Aliases.Add(newAlias);
        Aliases.AddRange(_cachedAliases);
        SelectedAlias = Aliases.Hydrate(selectedAlias);

        _thumbnailService.UpdateThumbnails(_cachedAliases);
        _logger.LogDebug("Loaded {Count} alias(es)", _cachedAliases.Count);

        SelectedAlias.MarkUnchanged();
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCurrentAlias))]
    private async Task OnLoadCurrentAliasAsync()
    {
        if ((SelectedAlias?.Id ?? 0) == 0) return;

        if (await CanSaveChanges(_previousAlias))
        {
            await SaveAliasAsync(_previousAlias!);
            return;
        }

        await ResetAlias(_previousAlias!);
        _previousAlias = SelectedAlias;
        SelectedAlias = await Task.Run(() => _aliasManagementService.Hydrate(SelectedAlias));
        SelectedAlias.MarkUnchanged();

        _logger.LogInformation("Loading alias {AliasName}", SelectedAlias.Name);

        return;

        async Task<bool> CanSaveChanges(AliasQueryResult? alias) => (alias?.IsDirty ?? false) &&
                                                                    await _hubService.Interactions.AskAsync(
                                                                        $"Do you want to save changes for '{alias.Name}'?"
                                                                    );
    }

    private async Task ResetAlias(AliasQueryResult? alias)
    {
        if ((alias?.Id ?? 0) == 0) return;
        
        alias.Rehydrate(
            _aliasManagementService.GetById(alias!.Id)
        );
        await _thumbnailService.UpdateThumbnailAsync(alias);
        await Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCurrentAlias))]
    private async Task OnSaveCurrentAliasAsync()
    {
        SelectedAlias = await SaveAliasAsync(SelectedAlias);
        _logger.LogDebug("Saved alias {AliasName}", SelectedAlias!.Name);
        await OnLoadAliases();
    }

    [RelayCommand]
    private void OnSearch(string criterion)
    {
        if (criterion.IsNullOrWhiteSpace())
        {
            Aliases = new(_cachedAliases);
            return;
        }

        criterion = criterion.ToLower();
        var aliases = _cachedAliases.Where(x => x.Name.StartsWith(criterion, StringComparison.CurrentCultureIgnoreCase))
                                    .ToArray();

        _logger.LogTrace("Found {Count} alias(es) with criterion {Criterion}", aliases.Length, criterion);
        Aliases = new(aliases);
    }

    [RelayCommand]
    private async Task OnSetPackagedApplication()
    {
        if (SelectedAlias is null) return;

        var viewModel = new UwpSelector { PackagedApps = await _packagedAppSearchService.GetInstalledUwpAppsAsync() };
        var result =  await _hubService.Interactions.InteractAsync(
            _viewFactory.CreateView(viewModel),
            "Select",
            ButtonLabels.Cancel,
            "Select UWP application"
        );

        if (!result.IsConfirmed) return;

        SelectedAlias.FileName = viewModel.SelectedPackagedApp?.AppUserModelId;
    }

    private async Task<AliasQueryResult?> SaveAliasAsync(AliasQueryResult? alias)
    {
        var result = _validationService.IsValid(SelectedAlias);
        if (!result.IsSuccess)
        {
            _hubService.Notifications.Warning(result.ErrorContent, "Validation failed");
            _logger.LogDebug("Validation failed for {AliasName}: {Errors}", SelectedAlias!.Name, result.ErrorContent);
            _hubService.Notifications.Warning($"Alias validation failed:\n{result.ErrorContent}");
            return alias;
        }

        await _packagedAppSearchService.TryResolveDetailsAsync(alias);

        await Task.Run(() => _aliasManagementService.SaveOrUpdate(ref alias));
        _hubService.Notifications.Success($"Alias {alias!.Name} updated or created.", "Item updated or created.");
        return alias;
    }

    #endregion
}