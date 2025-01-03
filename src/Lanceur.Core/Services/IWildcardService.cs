namespace Lanceur.Core.Services;

public interface IWildcardService
{
    #region Methods

    /// <summary>
    /// Executes all the replacement actions.
    /// </summary>
    /// <param name="text">The text in which the replacement will occur</param>
    /// <param name="replacement">The replacement text</param>
    /// <returns>
    /// The modified text after applying all replacements
    /// </returns>
    string Replace(string text, string replacement);

    /// <summary>
    /// Executes all the replacement actions, or returns the replacement text if the provided <paramref name="text"/> is null.
    /// </summary>
    /// <param name="text">The parameters as specified in the Alias</param>
    /// <param name="replacement">The replacement text</param>
    /// <returns>
    /// The modified text if replacements are performed, or the <paramref name="replacement"/> if <paramref name="text"/> is null
    /// </returns>
    string ReplaceOrReplacementOnNull(string text, string replacement);

    #endregion Methods
}