using System.Windows.Input;
using Lanceur.Core.Configuration.Sections;

namespace Lanceur.Ui.WPF.Extensions;

public static class WpfExtensions
{
    #region Methods

    public static string ToStringHotKey(this HotKeySection src)
    {
        var mk = ((ModifierKeys)src.ModifierKey).ToString();
        var k = ((Key)src.Key).ToString();
        return $"{mk} + {k}";
    }

    #endregion
}