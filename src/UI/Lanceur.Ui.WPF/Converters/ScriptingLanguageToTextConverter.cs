using System.Globalization;
using System.Windows.Data;
using Lanceur.Core.Configuration.Sections;

namespace Lanceur.Ui.WPF.Converters;

internal class ScriptingLanguageToTextConverter : IValueConverter
{
    #region Fields

    private const string CSharpLanguage = "C# Scripting";
    private const string LuaLanguage = "Lua";

    #endregion

    #region Methods

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ScriptLanguage language) return Binding.DoNothing;

        return language switch
        {
            ScriptLanguage.Lua             => LuaLanguage,
            ScriptLanguage.CSharpScripting => CSharpLanguage,
            _                              => Binding.DoNothing
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string language) return Binding.DoNothing;

        return language switch
        {
            CSharpLanguage => ScriptLanguage.CSharpScripting,
            LuaLanguage    => ScriptLanguage.Lua,
            _              => Binding.DoNothing
        };
    }

    #endregion
}