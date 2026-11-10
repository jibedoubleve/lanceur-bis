namespace Lanceur.Core.Services;

public interface IWildcardService
{
    #region Methods

    /// <summary>
    ///     Indicates whether <paramref name="template" /> contains at least one wildcard.
    /// </summary>
    /// <param name="template">The text to inspect.</param>
    /// <returns><c>true</c> if a wildcard is found; otherwise, <c>false</c>.</returns>
    bool HasWildcard(string template);

    /// <summary>
    ///     Replaces every wildcard in <paramref name="template" /> with its resolved value.
    /// </summary>
    /// <remarks>
    ///     A wildcard is a single letter between dollar signs, case-insensitive:
    ///     <list type="bullet">
    ///         <item><c>$I$</c>: the user input, as is.</item>
    ///         <item><c>$W$</c>: the user input, URL-encoded and lowercased.</item>
    ///         <item><c>$R$</c>: the clipboard text, as is.</item>
    ///         <item><c>$C$</c>: the clipboard text, URL-encoded.</item>
    ///     </list>
    ///     Any other letter is replaced with the user input.
    /// </remarks>
    /// <param name="template">The text containing the wildcards to replace.</param>
    /// <param name="input">The user input substituted for <c>$I$</c> and <c>$W$</c>.</param>
    /// <returns>The template with all its wildcards replaced.</returns>
    string Replace(string template, string input);

    #endregion
}