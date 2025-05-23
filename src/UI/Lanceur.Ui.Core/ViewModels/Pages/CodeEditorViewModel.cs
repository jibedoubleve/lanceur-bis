using CommunityToolkit.Mvvm.ComponentModel;
using Lanceur.Core.Models;
using Lanceur.SharedKernel.DI;

namespace Lanceur.Ui.Core.ViewModels.Pages;

[Singleton]
public partial class CodeEditorViewModel : ObservableObject
{
    #region Fields

    [ObservableProperty] private AliasQueryResult _alias = new();

    #endregion

    #region Properties

    public KeywordsViewModel PreviousViewModel { get; set; } = null!;

    #endregion

    #region Methods

    public void SetLuaScript(string luaScript) => Alias.LuaScript = luaScript;

    #endregion
}