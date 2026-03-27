namespace Lanceur.Core.Configuration.Sections.Application;

public record HotKeySection
{
    #region Constructors

    public HotKeySection(int modifierKey, int key)
    {
        Key = key;
        ModifierKey = modifierKey;
    }

    #endregion

    #region Properties

    /// <remarks>
    ///     Setter is used for serialization
    /// </remarks>
    public int Key { get; set; }

    /// <remarks>
    ///     Setter is used for serialization
    /// </remarks>
    public int ModifierKey { get; set; }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override int GetHashCode() => (Key, ModifierKey).GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"Key: {Key} | Modifier: {ModifierKey}";

    #endregion
}