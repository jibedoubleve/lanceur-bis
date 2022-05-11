namespace Lanceur.Core.Models
{
    public class CompositeAliasQueryResult : AliasQueryResult
    {
        #region Constructors

        public CompositeAliasQueryResult(IEnumerable<AliasQueryResult> aliases)
        {
            Aliases = aliases;
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<AliasQueryResult> Aliases { get; }

        #endregion Properties

        #region Methods

        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            foreach (var alias in Aliases)
            {
                await alias.ExecuteAsync(cmdline);
            }
            return NoResult;
        }

        #endregion Methods
    }
}