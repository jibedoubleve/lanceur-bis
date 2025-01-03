using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;

namespace Lanceur.Ui.Core.ViewModels.Pages;

public partial class CodeEditorViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private AliasQueryResult _alias = new();

    public void SetLuaScript(string luaScript) => Alias.LuaScript = luaScript;
    
    #endregion
}