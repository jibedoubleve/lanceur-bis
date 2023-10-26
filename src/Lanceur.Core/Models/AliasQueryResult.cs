using System.Collections.ObjectModel;
using System.Diagnostics;
using Lanceur.SharedKernel.Mixins;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Core.Models;

[DebuggerDisplay("Name: {Name} - Synonyms: {Synonyms}")]
public class AliasQueryResult : ExecutableQueryResult, IElevated
{
    #region Fields

    private string _fileName;

    private string _synonyms;

    private ObservableCollection<QueryResultAdditionalParameters> _additionalParameters = new();

    #endregion Fields

    #region Properties

    /// <summary>
    /// Additional parameters are a way to not duplicate alias to create other with the same
    /// configuration but with only one parameter that differs
    /// </summary>
    public ObservableCollection<QueryResultAdditionalParameters> AdditionalParameters
    {
        get => _additionalParameters;
        set => Set(ref _additionalParameters, value);
    }

    public static AliasQueryResult EmptyForCreation => new() { Name = "new alias" };

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

    public bool IsHidden { get; set; } = false;

    public string Notes { get; set; }

    public RunAs RunAs { get; set; } = RunAs.CurrentUser;

    public StartMode StartMode { get; set; } = StartMode.Default;

    /// <summary>
    /// Synonyms present when the entity was loaded
    /// </summary>
    public string SynonymsPreviousState { get; set; }

    /// <summary>
    /// Get or set a string representing all the name this alias has.
    /// It should be a coma separated list of names.
    /// </summary>
    public string Synonyms
    {
        get => _synonyms;
        set
        {
            Set(ref _synonyms, value);
            OnPropertyChanged(nameof(SynonymsNextState));
        }
    }

    /// <summary>
    /// New synonyms added when updated
    /// </summary>
    public string SynonymsNextState => (from n in Synonyms.SplitCsv()
                                        where !SynonymsPreviousState.SplitCsv().Contains(n)
                                        select n).ToArray().JoinCsv();

    public string WorkingDirectory { get; set; }

    #endregion Properties

    #region Methods

    public static AliasQueryResult FromName(string aliasName) => new() { Name = aliasName, Synonyms = aliasName };

    public override int GetHashCode() => (base.GetHashCode(), (Parameters?.GetHashCode() ?? 0)).GetHashCode();

    #endregion Methods
}