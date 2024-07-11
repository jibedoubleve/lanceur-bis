using Lanceur.SharedKernel.Mixins;
using System.Text.RegularExpressions;

namespace Lanceur.Core.Models
{
    public static class QueryResultMixin
    {
        #region Fields

        private static readonly Regex Regex = new("@([a-zA-Z]*)@");

        #endregion Fields

        #region Methods

        private static bool Is(this AliasQueryResult @this, CompositeMacros macro) => @this.FileName.ToLower().Contains($"@{macro.ToLowerString()}@".ToLower());

        public static string GetMacroName(this AliasQueryResult @this)
        {
            if (@this is null) return string.Empty;

            var matches = Regex.Match(@this.FileName ?? string.Empty);
            var result = matches.Success ? matches.Groups[1].Value : string.Empty;
            return result.ToUpper();
        }

        public static bool IsComposite(this AliasQueryResult @this) => @this.Is(CompositeMacros.Multi);

        public static bool IsMacro(this AliasQueryResult @this) => !GetMacroName(@this).IsNullOrEmpty();

        #endregion Methods
    }
}