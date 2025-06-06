using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Lanceur.Core.LuaScripting;
using Lanceur.Core.Services;
using Lanceur.Infra.LuaScripting;
using Lanceur.SharedKernel.Extensions;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.Services;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Views.Pages;

public partial class CodeEditorView
{
    #region Fields

    private readonly ILogger<CodeEditorView> _logger;
    private readonly IUserNotificationService _userNotificationService;
    private readonly ILuaManager _luaManager;

    #endregion

    #region Constructors

    public CodeEditorView(CodeEditorViewModel viewModel, ILogger<CodeEditorView> logger, IUserNotificationService userNotificationService, ILuaManager luaManager)
    {
        _logger = logger;
        _userNotificationService = userNotificationService;
        _luaManager = luaManager;
        DataContext = ViewModel = viewModel;
        InitializeComponent();
    }

    #endregion

    #region Properties

    private CodeEditorViewModel ViewModel { get; set; }

    #endregion

    #region Methods

    private void ApplyScript()
    {
        ViewModel.SetLuaScript(LuaEditor.Text);
        GoBack();
    }

    private void GoBack() => WeakReferenceMessenger.Default.Send(new NavigationMessage((ViewType: typeof(KeywordsView), DataContext: ViewModel.PreviousViewModel)!));

    private void OnClickApply(object sender, RoutedEventArgs e) => ApplyScript();

    private void OnClickRollback(object sender, RoutedEventArgs e) => GoBack();

    private void OnClickRun(object sender, RoutedEventArgs e) => RunScript();

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5) RunScript();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ScriptOutput.Content
            = ScriptErrorOutput.Content
                = string.Empty;

        SetupSyntaxColouration();
        var vm = (CodeEditorViewModel)DataContext;

        LuaEditor.Text = vm.Alias.LuaScript;
        TbFileName.Text = vm.Alias.FileName;
        TbParameters.Text = vm.Alias.Parameters;

        _logger.LogDebug("{View} has loaded lua script for {Alias}", GetType().Name, vm.Alias.Name);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5) e.Handled = true;
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) GoBack();
    }

    private void RunScript()
    {
        ScriptOutput.Content = string.Empty;
        ScriptErrorOutput.Content = string.Empty;

        var inputParameters = TbParameters.Text.IsNullOrWhiteSpace() ? ViewModel.Alias.Parameters : TbParameters.Text;
        var inputFileName = TbFileName.Text.IsNullOrWhiteSpace() ? ViewModel.Alias.FileName : TbFileName.Text;
        var script = new Script { Code = LuaEditor.Text, Context = new() { Parameters = inputParameters, FileName = inputFileName } };
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

        _userNotificationService.Success("Script executed successfully in dry run mode.", "Build Successful");
    }

    private void SetupSyntaxColouration()
    {
        const string res = "Lanceur.Ui.WPF.SyntaxColouration.LUA-Mode.xml";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res) ?? throw new NotSupportedException($"Cannot find resource '{res}'");
        using var reader = new XmlTextReader(stream);
        LuaEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    #endregion
}