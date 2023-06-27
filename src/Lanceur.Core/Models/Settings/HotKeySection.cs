namespace Lanceur.Core.Models.Settings
{
    public class HotKeySection
    {
        #region Constructors

        private HotKeySection()
        { }

        public HotKeySection(int modifier, int key)
        {
            Key = key;
            ModifierKey = modifier;
        }

        #endregion Constructors

        #region Properties

        public static HotKeySection Default => new(3, 18);
        public int Key { get; set; }
        public int ModifierKey { get; set; }

        #endregion Properties

        #region Methods

        ///<inheritdoc/>
        public override int GetHashCode() => (Key, ModifierKey).GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => $"Key: {Key} | Modifier: {ModifierKey}";

        #endregion Methods
    }
}