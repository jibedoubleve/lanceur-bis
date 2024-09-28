using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Lanceur.Ui.WPF.SyntaxColouration;

public class LuaCompletionData : ICompletionData
{
    #region Constructors

    public LuaCompletionData(string text, string description = null, double priority = 1)
    {
        Text = text;
        Description = description;
        Priority = priority;
    }

    #endregion Constructors

    #region Properties

    // Use this property if you want to show a fancy UIElement in the list.
    public object Content => Text;

    public object Description { get; }
    public System.Windows.Media.ImageSource Image => null;
    public double Priority { get; }
    public string Text { get; }

    #endregion Properties

    #region Methods

    public void Complete(
        TextArea textArea,
        ISegment completionSegment,
        EventArgs insertionRequestEventArgs
    )
    {
        textArea.Document.Replace(completionSegment, Text);
    }

    #endregion Methods
}