using Lanceur.Core.Models.Settings;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace Lanceur.Converters.Reactive
{
    public static class MahAppHotKeyConverterMixin
    {
        #region Methods

        public static HotKeySection ToHotKeySection(this HotKey hk)
        {
            return hk is not null
                   ? new((int)hk.ModifierKeys, (int)hk.Key)
                   : null;
        }

        public static HotKey ToMahAppHotKey(this HotKeySection hk)
        {
            return hk is not null
              ? new((Key)hk.Key, (ModifierKeys)hk.ModifierKey)
              : null;
        }

        #endregion Methods
    }
}