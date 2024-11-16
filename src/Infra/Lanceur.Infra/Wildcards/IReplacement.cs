namespace Lanceur.Infra.Wildcards;

public interface IReplacement
{
    #region Properties

    /// <summary>
    /// Indicates the value of the wildcards
    /// </summary>
    string Wildcard { get; }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Replace off the occurence of <see cref="Wildcard"/> with
    /// <paramref name="replacement"/> in the specified <paramref name="text"/>
    /// </summary>
    /// <param name="text"></param>
    /// <param name="replacement"></param>
    /// <returns></returns>
    string Replace(string text, string replacement);

    #endregion Methods
}