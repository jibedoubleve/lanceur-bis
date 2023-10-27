namespace Lanceur.Core.Managers
{
    public interface IWildcardManager
    {
        #region Methods

        /// <summary>
        /// Will go through all the replacement actions and execute them.
        /// A replacement action is implementing <see cref="IReplacement"/>
        /// </summary>
        /// <param name="text">The text where the replacement will take place</param>
        /// <param name="replacement">The replacement text</param>
        /// <returns></returns>
        string Replace(string text, string replacement);

        /// <summary>
        /// Will go through all the replacement actions and execute them.
        /// A replacement action is implementing <see cref="IReplacement"/>.
        /// If the specified <paramref name="text"/> is null, then return the
        /// <paramref name="replacement"/>
        /// </summary>
        /// <param name="text">The parameters as specified in the Alias</param>
        /// <param name="replacement">The parameters specified by the user in the Query</param>
        /// <returns>
        /// The parameters to apply to the execution of the alias
        /// </returns>
        string ReplaceOrReplacementOnNull(string text, string replacement);

        #endregion Methods
    }
}