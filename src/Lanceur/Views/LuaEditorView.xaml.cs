using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Lanceur.Core.LuaScripting;
using Lanceur.Infra.LuaScripting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Lanceur.SyntaxColoration;

namespace Lanceur.Views
{
    /// <summary>
    /// Interaction logic for LuaEditorView.xaml
    /// </summary>
    public partial class LuaEditorView
    {
        #region Fields

        private CompletionWindow _completionWindow;
        private Script _luaCode;

        #endregion Fields

        #region Constructors

        public LuaEditorView()
        {
            InitializeComponent();

            Editor.TextArea.TextEntered += OnTextEntered;
            Editor.TextArea.TextEntering += OnTextEntering;
        }

        #endregion Constructors

        #region Properties

        public Script LuaScript
        {
            get => _luaCode.Clone(Editor.Text);
            set
            {
                _luaCode = value;
                Editor.Text = value.Code;
                InputFileName.Text = value.Context.FileName;
                InputParameters.Text = value.Context.Parameters;
            }
        }

        #endregion Properties

        #region Methods

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnClickRun(object sender, RoutedEventArgs e)
        {
            ScriptOutput.Text = string.Empty;
            ErrorOutput.Text = string.Empty;

            var script = new Script
            {
                Code = Editor.Text,
                Context = new()
                {
                    Parameters = InputParameters.Text,
                    FileName = InputFileName.Text,
                },
            };
            var result = LuaManager.ExecuteScript(script);

            if (!string.IsNullOrEmpty(result.Error))
            {
                ErrorOutputHeader.Visibility = Visibility.Visible;
                ScriptOutputHeader.Visibility = Visibility.Collapsed;
                ErrorOutput.Text = result.Error;
                return;
            }

            ErrorOutputHeader.Visibility = Visibility.Collapsed;
            ScriptOutputHeader.Visibility = Visibility.Visible;
            ScriptOutput.Text = result.ToString();
        }

        private void OnClickSave(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            const string res = "Lanceur.SyntaxColoration.LUA-Mode.xml";

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res)
                ?? throw new NullReferenceException($"Cannot find resource '{res}'");
            using var reader = new XmlTextReader(stream);
            Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }

        private void OnTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != ".") return;

            // Open code completion after the user has pressed dot
            _completionWindow = new(Editor.TextArea);
            var data = _completionWindow.CompletionList.CompletionData;
            data.FillContextFields();

            _completionWindow.Show();
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }

        private void OnTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length <= 0 || _completionWindow == null) return;
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                _completionWindow.CompletionList.RequestInsertion(e);
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        #endregion Methods
    }
}