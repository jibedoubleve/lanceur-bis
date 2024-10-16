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
using Lanceur.SharedKernel.Mixins;
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

    #endregion Fields

    #region Constructors

    public CodeEditorView(CodeEditorViewModel viewModel, ILogger<CodeEditorView> logger, IUserNotificationService userNotificationService)
    {
        _logger = logger;
        _userNotificationService = userNotificationService;
        DataContext = ViewModel = viewModel;
        InitializeComponent();
    }

    #endregion Constructors

    #region Properties

    private CodeEditorViewModel ViewModel { get; set; }

    #endregion Properties

    #region Methods

    private void OnClickRollback(object sender, RoutedEventArgs e) => GoBack();

    private void OnClickRun(object sender, RoutedEventArgs e) => RunScript();

    private void OnClickApply(object sender, RoutedEventArgs e) => ApplyScript();

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5) { RunScript(); }
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

        _logger.LogTrace("{View} has loaded lua script for {Alias}", GetType().Name, vm.Alias.Name);
    }
    
    private void GoBack() => WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(KeywordsView)));

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5) { e.Handled = true; }
    }

    private void RunScript()
    {
        ScriptOutput.Content = string.Empty;
        ScriptErrorOutput.Content = string.Empty;

        var inputParameters = TbParameters.Text.IsNullOrWhiteSpace() ? ViewModel.Alias.Parameters : TbParameters.Text;
        var inputFileName = TbFileName.Text.IsNullOrWhiteSpace() ? ViewModel.Alias.FileName : TbFileName.Text;
        var script = new Script
        {
            Code = LuaEditor.Text,
            Context = new()
            {
                Parameters = inputParameters,
                FileName = inputFileName
            }
        };
        var result = LuaManager.ExecuteScript(script);

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

    private void ApplyScript()
    {
        ViewModel.SetLuaScript(LuaEditor.Text);
        GoBack();
    }

    private void SetupSyntaxColouration()
    {
        const string res = "Lanceur.Ui.WPF.SyntaxColouration.LUA-Mode.xml";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res) ?? throw new NullReferenceException($"Cannot find resource '{res}'");
        using var reader = new XmlTextReader(stream);
        LuaEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    #endregion Methods

}