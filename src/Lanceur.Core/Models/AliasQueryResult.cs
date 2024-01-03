using Lanceur.SharedKernel.Mixins;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Core.Models;

[DebuggerDisplay("{Id} - Name: {Name} - Synonyms: {Synonyms}")]
public class AliasQueryResult : ExecutableQueryResult, IElevated
{
    #region Fields

    private ObservableCollection<QueryResultAdditionalParameters> _additionalParameters = new();
    private string _fileName;

    private string _synonyms;

    #endregion Fields

    #region Properties

    public static AliasQueryResult EmptyForCreation => new() { Name = "new alias" };

    public new static IEnumerable<AliasQueryResult> NoResult => new List<AliasQueryResult>();

    /// <summary>
    /// Additional parameters are a way to not duplicate alias to create other with the same
    /// configuration but with only one parameter that differs
    /// </summary>
    public ObservableCollection<QueryResultAdditionalParameters> AdditionalParameters
    {
        get => _additionalParameters;
        set => Set(ref _additionalParameters, value);
    }

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
    /// in privilege mode (as admin).
    /// </summary>
    public override bool IsElevated
    {
        get => RunAs.Admin == RunAs;
        set => RunAs = value ? RunAs.Admin : RunAs.CurrentUser;
    }

    public bool IsHidden { get; set; }

    /// <summary>
    /// Gets or sets a Lua script that will be executed
    /// when user launch an alias
    /// </summary>
    public string LuaScript { get; set; }

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
        set
        {
            var trimmed = value.Trim(',', ' ');
            Set(ref _synonyms, trimmed);
            OnPropertyChanged(nameof(SynonymsToAdd));
        }
    }

    /// <summary>
    /// New synonyms added when updated
    /// </summary>
    public string SynonymsToAdd => (from n in Synonyms.SplitCsv()
                                    where !SynonymsWhenLoaded.SplitCsv().Contains(n)
                                    select n).ToArray().JoinCsv();

    /// <summary>
    /// Synonyms present when the entity was loaded
    /// </summary>
    public string SynonymsWhenLoaded { get; set; }

    public string WorkingDirectory { get; set; }

    #endregion Properties

    #region Methods

    public static AliasQueryResult FromName(string aliasName) => new() { Name = aliasName, Synonyms = aliasName };

    #endregion Methods
}