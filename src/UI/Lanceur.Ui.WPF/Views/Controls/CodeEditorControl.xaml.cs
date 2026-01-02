using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections;
using Lanceur.Core.Models;
using Lanceur.Core.Scripting;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.IoC;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Views.Controls;

[Singleton]
public partial class CodeEditorControl
{
    #region Fields

    private readonly ILogger<CodeEditorControl> _logger;
    private string? _scriptCache;

    private readonly IScriptEngineFactory _scriptEngineFactory;
    private readonly ISection<ScriptingSection> _settings;
    private readonly IUserNotificationService _userNotificationService;

    #endregion

    #region Constructors

    public CodeEditorControl(
        IUserNotificationService userNotificationService,
        IScriptEngineFactory scriptEngineFactoryFactory,
        ISection<ScriptingSection> settings,
        ILogger<CodeEditorControl> logger
    )
    {
        _userNotificationService = userNotificationService;
        _scriptEngineFactory = scriptEngineFactoryFactory;
        _settings = settings;
        _logger = logger;
        InitializeComponent();
    }

    #endregion

    #region Methods

    private async void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        try { await RunScriptAsync(); }
        catch (Exception ex) { _logger.LogWarning(ex, "Unhandled exception when executing script."); }
    }

    private void LogToggleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        LogToggleMenuItem.Header = LogToggleMenuItem.IsChecked
            ? "Log activated" 
            : "Log deactivated";
    }

    private async void OnKeyUp(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.F5) await RunScriptAsync();
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Unhandled exception when executing script."); }
    }

    private async Task RunScriptAsync()
    {
        ScriptOutput.Text = string.Empty;
        ScriptErrorOutput.Text = string.Empty;

        var inputParameters = TbParameters.Text;
        var inputFileName = TbFileName.Text;
        var script = new Script
        {
            Code = CodeEditor.Text, 
            Context = new() { Parameters = inputParameters, FileName = inputFileName }
        };

        var isDebug = LogToggleMenuItem.IsChecked; 
        var result = await _scriptEngineFactory.Current.ExecuteScriptAsync(script, isDebug);

        if (result.Exception is not null)
        {
            ScriptErrorOutput.Visibility = Visibility.Visible;
            ScriptOutput.Visibility = Visibility.Collapsed;
            ScriptErrorOutput.Text = result.Exception.Message;
            return;
        }

        ScriptErrorOutput.Visibility = Visibility.Collapsed;
        ScriptOutput.Visibility = Visibility.Visible;
        ScriptOutput.Text = $"""
                             INPUT
                             =====

                             File name  : {inputFileName}
                             Parameters : {inputParameters}

                             ---------------------------------------
                             OUTPUT
                             =====

                             {result}

                             """;

        _userNotificationService.Success("Script executed successfully in dry run mode.", "Build Successful");
    }

    private void SetupSyntaxColouration(string? res)
    {
        if (res is null)
        {
            _logger.LogInformation("Script engine is unknown, no syntactic colouration is configured.");
            return;
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res) ??
                           throw new NotSupportedException($"Cannot find resource '{res}'");
        using var reader = new XmlTextReader(stream);
        CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    public string Apply() => CodeEditor.Text;

    public void Load(AliasQueryResult alias)
    {
        ScriptOutput.Text
            = ScriptErrorOutput.Text
                = string.Empty;

        var syntax = _settings.Value.ScriptLanguage switch
        {
            ScriptLanguage.Lua             => "Lanceur.Ui.WPF.SyntaxColouration.LUA-Mode.xml",
            ScriptLanguage.CSharpScripting => "Lanceur.Ui.WPF.SyntaxColouration.CSharp-Mode.xml",
            _                              => null
        };
        SetupSyntaxColouration(syntax);

        _scriptCache = alias.Script;
        CodeEditor.Text = alias.Script;
        TbFileName.Text = alias.FileName ?? string.Empty;
        TbParameters.Text = alias.Parameters ?? string.Empty;
    }

    public string Reset() => _scriptCache ?? string.Empty;

    #endregion
}