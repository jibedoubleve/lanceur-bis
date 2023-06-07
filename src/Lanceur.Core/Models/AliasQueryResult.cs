using Lanceur.SharedKernel.Mixins;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Core.Models
{
    public class AliasQueryResult : ExecutableQueryResult, IElevated
    {
        #region Fields

        private string _fileName;

        private string _synonyms;

        #endregion Fields

        #region Properties

        public static AliasQueryResult EmptyForCreation => new() { Name = $"new alias" };

        public new static IEnumerable<AliasQueryResult> NoResult => new List<AliasQueryResult>();

        public int Delay { get; set; }

        public override string Description
        {
            get => Notes.IsNullOrWhiteSpace() ? FileName : Notes;
            set => Notes = value;
        }

        public string FileName
        {
            get => _fileName;
            set => Set(ref _fileName, value);
        }

        /// <summary>
        /// Indicates whether the alias should override the RunAs value and execute
        /// in priviledge mode (as admin).
        /// </summary>
        public override bool IsElevated
        {
            get => RunAs.Admin == RunAs;
            set => RunAs = value ? RunAs.Admin : RunAs.CurrentUser;
        }

        public string Notes { get; set; }

        public RunAs RunAs { get; set; } = RunAs.CurrentUser;

        public StartMode StartMode { get; set; } = StartMode.Default;

        /// <summary>
        /// Get or set a string representing all the name this alias has.
        /// It should be a coma separated list of names.
        /// </summary>
        public string Synonyms
        {
            get => _synonyms;
            set => Set(ref _synonyms, value);
        }

        public string WorkingDirectory { get; set; }

        #endregion Properties

        #region Methods

        public static AliasQueryResult FromName(string aliasName) => new AliasQueryResult() { Name = aliasName, Synonyms = aliasName };

        #endregion Methods
    }
}