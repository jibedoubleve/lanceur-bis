using Lanceur.Core.Models;
using System.Collections.Generic;

namespace Lanceur.Models
{
    internal static class ModelMixin
    {
        #region Fields

        private const string Dark = "contrast-black";
        private const string Light = "contrast-white";

        #endregion Fields

        #region Methods

        public static IEnumerable<QueryResult> SetIconForCurrentTheme(this IEnumerable<QueryResult> result, bool isLight)
        {
            var oldValue = isLight ? Dark : Light;
            var newValue = isLight ? Light : Dark; 

            foreach (var item in result)
            {
                item.Icon = item.Icon.Replace(oldValue, newValue);
            }

            return result;
        }

        #endregion Methods
    }
}