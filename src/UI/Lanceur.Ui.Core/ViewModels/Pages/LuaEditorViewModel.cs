using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.IoC;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class LuaEditorViewModel : ObservableObject
{
    #region Fields

    private readonly ILuaManager _luaManager;
    private readonly IUserCommunicationService _hubService;

    private AliasQueryResult? _alias;
    [ObservableProperty] private string _luaScript = string.Empty;
    private string _originalScript = string.Empty;
    [ObservableProperty] private string _parameters = string.Empty;
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private string _output = "No output ...";
    [ObservableProperty] private string _logs = "No log content...";
    [ObservableProperty] private string? _errorOutput;
    [ObservableProperty] private bool _hasError;

    #endregion

    #region Constructors

    public LuaEditorViewModel(
        ILuaManager luaManager,
        IUserCommunicationService hubService)
    {
        _luaManager = luaManager;
        _hubService = hubService;
    }

    #endregion

    #region Properties

    public bool HasChanges => LuaScript != _originalScript;

    #endregion

    #region Methods

    public void Load(AliasQueryResult alias)
    {
        _alias = alias;
        _originalScript = alias.LuaScript ?? string.Empty;
        LuaScript = alias.LuaScript ?? string.Empty;
        FileName = alias.FileName ?? string.Empty;
        Parameters = alias.Parameters ?? string.Empty;

        // Reset output
        Output = "No output ...";
        Logs = "No log content...";
        ErrorOutput = null;
        HasError = false;
    }

    [RelayCommand]
    private void OnDryRun()
    {
        ErrorOutput = null;
        HasError = false;
        Output = string.Empty;

        var script = new Script
        {
            Code = LuaScript,
            Context = new() { Parameters = Parameters, FileName = FileName }
        };
        var result = _luaManager.ExecuteScript(script);

        if (result.Exception is not null)
        {
            HasError = true;
            ErrorOutput = result.Exception.Message;
            return;
        }

        Output = $"""
                  INPUT
                  =====

                  File name  : {FileName}
                  Parameters : {Parameters}

                  ---------------------------------------
                  OUTPUT
                  =====

                  {result}

                  """;
        Logs = result.OutputContent ?? "No log content...";
        _hubService.Notifications.Success("Script executed successfully in dry run mode.", "Build Successful");
    }

    /// <summary>
    /// Saves the current Lua script to the alias.
    /// </summary>
    public void Save()
    {
        if (_alias is not null)
        {
            _alias.LuaScript = LuaScript;
            _alias.MarkChanged();
            _originalScript = LuaScript;
        }
    }

    /// <summary>
    /// Asks the user if they want to save unsaved changes.
    /// Returns true if the user wants to proceed with navigation, false to cancel.
    /// </summary>
    public async Task<bool> ConfirmDiscardOrSaveAsync()
    {
        if (!HasChanges) return true;

        var confirmed = await _hubService.Dialogues.AskAsync(
            "You have unsaved changes. Do you want to save before leaving?"
        );

        if (confirmed)
        {
            Save();
        }

        return true; // Always allow navigation after the dialog
    }

    #endregion
}
