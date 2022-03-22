using Lanceur.SharedKernel.Mixins;
using System.Text.RegularExpressions;

namespace Lanceur.Core.Models
{
    public static class QueryResultMixin
    {
        #region Methods

        public static string GetMacro(this AliasQueryResult @this)
        {
            var regex = new Regex("@(.*)@");
            var result = regex.IsMatch(@this?.FileName ?? string.Empty)
                ? regex.Match(@this.FileName ?? string.Empty).Groups[1].Value
                : string.Empty;
            return result.ToUpper();
        }


        public static bool Is(this AliasQueryResult @this, CompositeMacros macro) => @this.FileName.ToLower().Contains($"@{macro.ToLowerString()}@".ToLower());

        public static bool IsComposite(this AliasQueryResult @this)
        {
            return @this.Is(CompositeMacros.Multi);
        }

        public static bool IsMacro(this AliasQueryResult @this) => !GetMacro(@this).IsNullOrEmpty();

        public static string ToQuery(this QueryResult @this) => $"{@this.Name} {(@this.Query?.Parameters ?? "")}".Trim();

        #endregion Methods
    }
}