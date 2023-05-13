namespace Lanceur.Core.Models
{
    public abstract class SelfExecutableQueryResult : ExecutableQueryResult, ISelfExecutable
    {
        #region Constructors

        public SelfExecutableQueryResult()
        {
        }

        public SelfExecutableQueryResult(string name, string description)
        {
            Name = name;
            Description = description;
        }

        #endregion Constructors

        #region Methods

        public abstract Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null);

        public override string ToQuery() => $"{Name} {Parameters}".Trim();

        #endregion Methods
    }
}