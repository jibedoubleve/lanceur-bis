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
            Set(ref _synonyms, value);
            OnPropertyChanged(nameof(SynonymsToAdd));
        }
    }

    /// <summary>
    /// New synonyms added when updated
    /// </summary>
    public string SynonymsToAdd => Synonyms.SplitCsv()
                                           .Where(n => !SynonymsWhenLoaded.SplitCsv().Contains(n))
                                           .ToArray()
                                           .JoinCsv();

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

public static class AliasQueryResultExtensions
{
    /// <summary>
    /// Adds new synonyms to the specified alias while ensuring no duplicates exist.
    /// If any synonyms already exist, they will be ignored.
    /// </summary>
    /// <param name="alias">The alias object to update with new synonyms</param>
    /// <param name="synonyms">A collection of new synonyms to be added</param>
    public static void AddDistinctSynonyms(this AliasQueryResult alias, IEnumerable<string> synonyms)
    {
        var aggregation = alias.Synonyms.Split(",").Select(e => e.Trim()).ToList();
        aggregation.AddRange(synonyms);
        alias.Synonyms = string.Join(",", aggregation.Distinct());
    }

    /// <summary>
    /// Adds a collection of additional parameters to the AliasQueryResult object.
    /// </summary>
    /// <param name="alias">The AliasQueryResult object to which the additional parameters will be added.</param>
    /// <param name="additionalParameters">A collection of QueryResultAdditionalParameters to add to the alias.</param>

    public static void AdditionalParameters(this AliasQueryResult alias, IEnumerable<QueryResultAdditionalParameters> additionalParameters)
    {
        foreach (var additionalParameter in additionalParameters)
            alias.AdditionalParameters.Add(additionalParameter);
    }
}