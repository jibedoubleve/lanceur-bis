using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lanceur.Core.BusinessLogic;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.DI;
using Lanceur.SharedKernel.Mixins;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Utils;
using Lanceur.Ui.Core.ViewModels.Controls;
using Microsoft.Extensions.Logging;
using IUserNotificationService = Lanceur.Core.Services.IUserNotificationService;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class KeywordsViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private ObservableCollection<AliasQueryResult> _aliases = new();
    private readonly IAliasManagementService _aliasManagementService;
    private IList<AliasQueryResult> _bufferedAliases = Array.Empty<AliasQueryResult>();
    private readonly ILogger<KeywordsViewModel> _logger;
    private AliasQueryResult? _selectedAlias;
    private readonly IThumbnailService _thumbnailService;
    private readonly IUserInteractionService _userInteraction;
    private readonly IUserNotificationService _userNotificationService;
    private readonly IAliasValidationService _validationService;
    private readonly IViewFactory _viewFactory;

    #endregion

    #region Constructors

    public KeywordsViewModel(
        IAliasManagementService aliasManagementService,
        IThumbnailService thumbnailService,
        ILogger<KeywordsViewModel> logger,
        IUserNotificationService userNotificationService,
        IAliasValidationService validationService,
        IUserInteractionService userInteraction,
        IViewFactory viewFactory
    )
    {
        ArgumentNullException.ThrowIfNull(aliasManagementService);
        ArgumentNullException.ThrowIfNull(userNotificationService);
        ArgumentNullException.ThrowIfNull(thumbnailService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(validationService);
        ArgumentNullException.ThrowIfNull(userInteraction);
        ArgumentNullException.ThrowIfNull(viewFactory);

        WeakReferenceMessenger.Default.Register<AddAliasMessage>(this, (r, m) => ((KeywordsViewModel)r).CreateAlias(m));

        _userNotificationService = userNotificationService;
        _validationService = validationService;
        _userInteraction = userInteraction;
        _viewFactory = viewFactory;
        _aliasManagementService = aliasManagementService;
        _thumbnailService = thumbnailService;
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

    private void CreateAlias(AddAliasMessage message)
    {
        _logger.LogTrace("Creating new alias with name '{Name}'", message.Cmdline?.Parameters);
        var names = message.Cmdline?.Parameters;
        Aliases.Insert(0, new()  { Name = names, Synonyms = names });
        SelectedAlias = Aliases[0];
    }
    private bool CanExecuteCurrentAlias() => SelectedAlias != null;

    [RelayCommand]
    private async Task OnAddMultiParameters()
    {
        var viewModel = new MultipleAdditionalParameterViewModel();
        var view = _viewFactory.CreateView(viewModel);

        var result = await _userInteraction.AskUserYesNoAsync(view, "Apply", "Cancel", "Add multiple parameters");
        if (!result) return;

        var parser = new TextToParameterParser();
        var parseResult = parser.Parse(viewModel.RawParameters);

        if (!parseResult.Success)
        {
            _userNotificationService.Warn("The parsing operation failed because the entered text is invalid and cannot be converted into parameters.");
            return;
        }

        _selectedAlias?.AdditionalParameters.AddRange(parseResult.Parameters);
    }

    [RelayCommand]
    private async Task OnAddParameter()
    {
        var parameter = new AdditionalParameter();
        var view = _viewFactory.CreateView(parameter);

        var result = await _userInteraction.AskUserYesNoAsync(view, "Apply", "Cancel", "Add parameter");
        if (!result) return;

        SelectedAlias?.AdditionalParameters.Add(parameter);
        _userNotificationService.Success($"Parameter {parameter.Name} has been added. Don't forget to save to apply changes", "Updated.");
    }

    [RelayCommand]
    private void OnCreateAlias()
    {
        if (Aliases.Any(x => x.Id == 0))
        {
            SelectedAlias = Aliases.Single(x => x.Id == 0);
            _logger.LogTrace("Reselect alias to be created");
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
            _userNotificationService.Success($"Abort creation of alias {aliasName}.", "Cancel creation.");
            return;
        }

        var response = await _userInteraction.AskUserYesNoAsync($"Do you want to delete '{SelectedAlias.Name}'?");
        if (!response) return;

        _logger.LogTrace("Deleting alias {AliasName}", aliasName);
        await Task.Run(() => _aliasManagementService.Delete(SelectedAlias));
        var toDelete = Aliases.Where(x => x.Id == SelectedAlias.Id).ToArray();
        foreach (var item in toDelete) Aliases.Remove(item);
        _userNotificationService.Success($"Alias {aliasName} deleted.", "Item deleted.");
    }

    [RelayCommand]
    private async Task OnDeleteParameter(AdditionalParameter parameter)
    {
        var response = await _userInteraction.AskUserYesNoAsync(
            $"The parameter '{parameter.Name}' will disappear from the screen and be permanently deleted only after you save your changes. Do you want to continue?"
        );

        if (!response) return;

        var toRemove = SelectedAlias?.AdditionalParameters
                                    .Where(x => x.Id == parameter.Id)
                                    .ToArray()
                                    .SingleOrDefault();
        SelectedAlias?.AdditionalParameters.Remove(toRemove);
    }

    [RelayCommand]
    private async Task OnEditParameter(AdditionalParameter parameter)
    {
        var view = _viewFactory.CreateView(parameter);

        var result = await _userInteraction.AskUserYesNoAsync(view, "Apply", "Cancel", "Edit parameter");
        if (!result) return;

        var param = SelectedAlias?.AdditionalParameters?.Where(x => x.Id == parameter.Id).SingleOrDefault();
        if (param is null)
        {
            _logger.LogWarning("Parameter to update is not found in current alias.");
            return;
        }

        _userNotificationService.Success($"Modification has been done on {parameter.Name}. Don't forget to save to apply changes", "Updated.");
    }

    [RelayCommand]
    private async Task OnLoadAliases()
    {
        _bufferedAliases = await Task.Run(() => _aliasManagementService.GetAll()!.ToList());

        if (Aliases.Count == 0)
        {
            SelectedAlias = _bufferedAliases.ReselectAlias(SelectedAlias);
            Aliases = new(_bufferedAliases);
        }
        else
        {
            Aliases.AddRange(_bufferedAliases);
        }

        _thumbnailService.RefreshThumbnails(_bufferedAliases);
        _logger.LogTrace("Loaded {Count} alias(es)", _bufferedAliases.Count);
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
            _userNotificationService.Warn(result.ErrorContent, "Validation failed");
            _logger.LogInformation("Validation failed for {AliasName}: {Errors}", SelectedAlias!.Name, result.ErrorContent);
            return;
        }

        _logger.LogTrace("Saving alias {AliasName}", SelectedAlias!.Name);
        var alias = SelectedAlias;
        await Task.Run(() => _aliasManagementService.SaveOrUpdate(ref alias));
        _userNotificationService.Success($"Alias {alias.Name} created.", "Item created.");
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