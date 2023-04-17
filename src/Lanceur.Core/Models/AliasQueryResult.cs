using Lanceur.Core.Managers;
using Lanceur.SharedKernel.Mixins;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Core.Models
{
    public static class AliasQueryResultMixin
    {
        #region Methods

        public static AliasQueryResult Duplicate(this AliasQueryResult @this)
        {
            return new AliasQueryResult
            {
                Arguments = @this.Arguments,
                Count = @this.Count,
                FileName = @this.FileName,
                Icon = @this.Icon,
                Notes = @this.Notes,
                Name = $"Duplicate of {@this.Name}",
                RunAs = @this.RunAs,
                WorkingDirectory = @this.WorkingDirectory,
                ExecutionManager = @this.ExecutionManager,
                StartMode = @this.StartMode,
                Query = @this.Query,
                // In case it's already a duplicate of a duplicate,
                // go recursively all the way down to the original
                DuplicateOf = @this.DuplicateOf ?? @this.Id
            };
        }

        public static bool HasChangedName(this AliasQueryResult @this) => @this.Name?.ToLower() != @this.OldName.ToLower();

        #endregion Methods
    }

    public class AliasQueryResult : ExecutableWithPrivilege
    {
        #region Fields

        private string _fileName;

        #endregion Fields

        #region Properties

        internal IExecutionManager ExecutionManager { get; set; }
        internal Action<AliasQueryResult> OnExecution { get; set; }
        public static AliasQueryResult EmptyForCreation => new() { Name = $"new alias" };
        public new static IEnumerable<AliasQueryResult> NoResult => new List<AliasQueryResult>();
        public string Arguments { get; set; }

        public int Delay { get; set; }
        public override string Description => Notes.IsNullOrWhiteSpace() ? FileName : Notes;

        /// <summary>
        /// If this <see cref="AliasQueryResult"/> is a ducplicate from another
        /// alias, this contains its ID. Otherwise contains <c>NULL</c>
        /// </summary>
        public long? DuplicateOf { get; set; }

        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }

        /// <summary>
        /// Indicates whether the alias should override the RunAs value and execute
        /// in priviledge mode (as admin).
        /// </summary>
        public override bool IsPrivilegeOverriden
        {
            get;
            set;
        }

        public string Notes { get; set; }
        public RunAs RunAs { get; set; } = RunAs.CurrentUser;
        public StartMode StartMode { get; set; } = StartMode.Default;
        public string WorkingDirectory { get; set; }

        #endregion Properties

        #region Methods

        public static AliasQueryResult FromName(string aliasName, string description = null)
        {
            var result = new AliasQueryResult() { Name = aliasName };
            if (description is not null) { result.SetDescription(description); }
            return result;
        }

        public override async Task<IEnumerable<QueryResult>> ExecuteAsync(Cmdline cmdline = null)
        {
            await ExecutionManager?.ExecuteAsync(new ExecutionRequest
            {
                QueryResult = this,
                Query = (string)cmdline,
            });
            OnExecution(this);
            return NoResult;
        }

        #endregion Methods
    }
}