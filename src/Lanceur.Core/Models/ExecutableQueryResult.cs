﻿namespace Lanceur.Core.Models
{
    public abstract class ExecutableQueryResult : QueryResult, IExecutable
    {
        #region Fields

        private string _description;

        #endregion Fields

        #region Constructors

        public ExecutableQueryResult()
        {
        }

        public ExecutableQueryResult(string name, string description)
        {
            Name = name;
            _description = description;
        }

        #endregion Constructors

        #region Properties

        public override string Description => _description;

        #endregion Properties

        #region Methods

        public abstract Task<IEnumerable<QueryResult>> ExecuteAsync(string parameters = null);

        public void SetDescription(string description) => _description = description;

        #endregion Methods
    }
}