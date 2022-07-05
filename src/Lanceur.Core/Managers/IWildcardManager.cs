namespace Lanceur.Core.Managers
{
    public interface IWildcardManager
    {
        #region Methods

        /// <summary>
        /// Replace wildcards in text <paramref name="aliasParam"/> with <paramref name="userParam"/>
        /// and return the parameters to be applied in the execution of the alias.
        /// If the alias does not have parameter configured, then returns the user parameters
        /// contained into <paramref name="userParam"/>
        /// </summary>
        /// <param name="aliasParam">The parameters as specified in the Alias</param>
        /// <param name="userParam">The parameters specified by the user in the Query</param>
        /// <returns>
        /// The parameters to apply to the execution of the alias
        /// </returns>
        string HandleArgument(string aliasParam, string userParam);

        string Replace(string text, string param);

        #endregion Methods
    }
}