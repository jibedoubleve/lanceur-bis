namespace Lanceur.Core.Models.Settings;

public class HotKeySection
{
    #region Constructors

    public HotKeySection(int modifier, int key)
    {
        Key = key;
        ModifierKey = modifier;
    }

    #endregion

    #region Properties


    /// <remarks>
    ///     Setter is used for serialization
    /// </remarks>
    public int Key { get;  set; }

    /// <remarks>
    ///     Setter is used for serialization
    /// </remarks>
    public int ModifierKey { get;  set; }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override int GetHashCode() => (Key, ModifierKey).GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"Key: {Key} | Modifier: {ModifierKey}";

    #endregion
}