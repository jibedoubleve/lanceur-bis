using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.IoC;
using Lanceur.Ui.Core.Messages;
using Lanceur.Ui.Core.ViewModels.Pages;
using Microsoft.Extensions.Logging;

namespace Lanceur.Ui.WPF.Views.Pages;

[Singleton]
public partial class LuaEditorView
{
    #region Fields

    private bool _isUpdatingFromViewModel;

    private readonly ILogger<LuaEditorView> _logger;

    #endregion

    #region Constructors

    public LuaEditorView(
        LuaEditorViewModel viewModel,
        ILogger<LuaEditorView> logger
    )
    {
        _logger = logger;
        DataContext = ViewModel = viewModel;

        InitializeComponent();

        SetupSyntaxColouration();
        SetupEditorBinding();
    }

    #endregion

    #region Properties

    private LuaEditorViewModel ViewModel { get; }

    #endregion

    #region Methods

    private static void NavigateBackToKeywords()
    {
        WeakReferenceMessenger.Default.Send(
            new NavigationMessage((typeof(KeywordsView), null))
        );
    }

    private async void OnClickBack(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.SaveOrDiscardAsync();
            NavigateBackToKeywords();
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to navigate back: {ErrorMessage}", ex.Message); }
    }

    private void OnClickSave(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.Save();
            NavigateBackToKeywords();
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to save: {ErrorMessage}", ex.Message); }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5) { ViewModel.DryRunCommand.Execute(null); }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LuaEditorViewModel.LuaScript) || _isUpdatingFromViewModel) { return; }

        _isUpdatingFromViewModel = true;
        if (LuaEditor.Text != ViewModel.LuaScript) { LuaEditor.Text = ViewModel.LuaScript; }

        _isUpdatingFromViewModel = false;
    }

    private void SetupEditorBinding()
    {
        // AvalonEdit's Text property is not a DependencyProperty, so we need manual binding
        LuaEditor.TextChanged += (_, _) =>
        {
            if (!_isUpdatingFromViewModel) { ViewModel.LuaScript = LuaEditor.Text; }
        };

        // Update editor when ViewModel's LuaScript changes
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void SetupSyntaxColouration()
    {
        const string res = "Lanceur.Ui.WPF.SyntaxColouration.LUA-Mode.xml";

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res) ??
                           throw new NotSupportedException($"Cannot find resource '{res}'");
        using var reader = new XmlTextReader(stream);
        LuaEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    public void LoadAlias(AliasQueryResult alias)
    {
        ViewModel.Load(alias);
        _isUpdatingFromViewModel = true;
        LuaEditor.Text = ViewModel.LuaScript;
        _isUpdatingFromViewModel = false;
    }

    #endregion
}