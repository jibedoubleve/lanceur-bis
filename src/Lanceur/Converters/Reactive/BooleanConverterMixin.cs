using MahApps.Metro.IconPacks;

namespace Lanceur.Converters.Reactive
{
    internal static class BooleanConverterMixin
    {
        #region Methods

        public static PackIconModernKind ToIcon(this bool value) => value ? PackIconModernKind.Warning : PackIconModernKind.Magnify;

        #endregion Methods
    }
}