using System.Text.RegularExpressions;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Core.Models;

public static partial class QueryResultExtensions
{
    #region Fields

    private static readonly Regex FindMacroRegex = BuildFindMacroRegex();

    #endregion

    #region Methods

    [GeneratedRegex("@([a-zA-Z_]*)@")] private static partial Regex BuildFindMacroRegex();

    private static bool Is(this AliasQueryResult @this, CompositeMacros macro)
        => @this.FileName.ToLower().Contains($"@{macro.ToLowerString()}@".ToLower());

    public static string GetMacroName(this AliasQueryResult @this)
    {
        if (@this is null) { return string.Empty; }

        var matches = FindMacroRegex.Match(@this.FileName ?? string.Empty);
        var result = matches.Success ? matches.Groups[1].Value : string.Empty;
        return result.ToUpper();
    }

    public static bool IsComposite(this AliasQueryResult @this) => @this.Is(CompositeMacros.Multi);

    public static bool IsMacro(this AliasQueryResult @this) => !@this.GetMacroName().IsNullOrEmpty();

    #endregion
}