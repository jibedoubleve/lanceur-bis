using Lanceur.Core.Models;
using Lanceur.SharedKernel.Mixins;

namespace Lanceur.Core.Utils
{
    public static class AliasQueryResultMixin
    {
        #region Methods

        public static bool IsUwp(this AliasQueryResult src) => src.FileName.IsUwp();

        #endregion Methods
    }
}