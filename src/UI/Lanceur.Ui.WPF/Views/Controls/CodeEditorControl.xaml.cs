using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.SharedKernel.IoC;

namespace Lanceur.Ui.WPF.Views.Controls;

[Singleton]
public partial class CodeEditorControl
{
    #region Fields

    private readonly ILuaManager _luaManager;
    private string? _luaScriptCache;
    private readonly IUserNotificationService _userNotificationService;

    #endregion

    #region Constructors

    public CodeEditorControl(
        IUserNotificationService userNotificationService,
        ILuaManager luaManager
    )
    {
        _userNotificationService = userNotificationService;
        _luaManager = luaManager;
        InitializeComponent();
    }

    #endregion

    #region Methods

    private void MenuItem_Click(object sender, RoutedEventArgs e) => RunScript();

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5) RunScript();
    }

    private void RunScript()
    {
        ScriptOutput.Content = string.Empty;
        ScriptErrorOutput.Content = string.Empty;

        var inputParameters = TbParameters.Text;
        var inputFileName = TbFileName.Text;
        var script = new Script
        {
            Code = LuaEditor.Text, Context = new() { Parameters = inputParameters, FileName = inputFileName }
        };
        var result = _luaManager.ExecuteScript(script);

        if (result.Exception is not null)
        {
            ScriptErrorOutput.Visibility = Visibility.Visible;
            ScriptOutput.Visibility = Visibility.Collapsed;
            ScriptErrorOutput.Content = result.Exception.Message;
            return;
        }

        ScriptErrorOutput.Visibility = Visibility.Collapsed;
        ScriptOutput.Visibility = Visibility.Visible;
        ScriptOutput.Content = $"""
                                INPUT
                                =====

                                File name  : {inputFileName}
                                Parameters : {inputParameters}

                                ---------------------------------------
                                OUTPUT
                                =====

                                {result}

                                """;
        ScriptLogs.Content = result.OutputContent;

        _userNotificationService.Success("Script executed successfully in dry run mode.", "Build Successful");
    }

    private void SetupSyntaxColouration()
    {
        const string res = "Lanceur.Ui.WPF.SyntaxColouration.LUA-Mode.xml";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res) ??
                           throw new NotSupportedException($"Cannot find resource '{res}'");
        using var reader = new XmlTextReader(stream);
        LuaEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    public string Apply() => LuaEditor.Text;

    public void Load(AliasQueryResult alias)
    {
        ScriptOutput.Content
            = ScriptErrorOutput.Content
                = string.Empty;

        SetupSyntaxColouration();

        _luaScriptCache = alias?.LuaScript;
        LuaEditor.Text = alias?.LuaScript;
        TbFileName.Text = alias?.FileName ?? string.Empty;
        TbParameters.Text = alias?.Parameters ?? string.Empty;
    }

    public string Reset() => _luaScriptCache ?? string.Empty;

    #endregion
}