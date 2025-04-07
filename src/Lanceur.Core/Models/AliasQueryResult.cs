using System.Collections.ObjectModel;
using System.Diagnostics;
using Lanceur.SharedKernel.Extensions;
using static Lanceur.SharedKernel.Constants;

namespace Lanceur.Core.Models;

[DebuggerDisplay("{Id} - Name: {Name} - Synonyms: {Synonyms}")]
public class AliasQueryResult : ExecutableQueryResult, IElevated
{
    #region Fields

    private ObservableCollection<AdditionalParameter> _additionalParameters = new();
    private string _fileName;

    private string _synonyms;

    #endregion

    #region Properties

    /// <summary>
    ///     Additional parameters are a way to not duplicate alias to create other with the same
    ///     configuration but with only one parameter that differs
    /// </summary>
    public ObservableCollection<AdditionalParameter> AdditionalParameters
    {
        get => _additionalParameters;
        set => SetField(ref _additionalParameters, value);
    }

    public int Delay { get; set; }

    public override string DescriptionDisplay => Description.IsNullOrEmpty() ? FileName : Description;

    public static AliasQueryResult EmptyForCreation => new() { Name = "new alias" };

    public string FileName
    {
        get => _fileName;
        set => SetField(ref _fileName, value);
    }

    /// <summary>
    ///     Indicates whether the alias should override the RunAs value and execute
    ///     in privilege mode (as admin).
    /// </summary>
    public override bool IsElevated
    {
        get => RunAs.Admin == RunAs;
        set => RunAs = value ? RunAs.Admin : RunAs.CurrentUser;
    }

    public bool IsHidden { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when this alias was last used.
    ///     This value is used to track alias activity and determine usage recency.
    /// </summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    ///     Gets or sets a Lua script that will be executed
    ///     when user launch an alias
    /// </summary>
    public string LuaScript { get; set; }

    public new static IEnumerable<AliasQueryResult> NoResult => new List<AliasQueryResult>();

    public RunAs RunAs { get; set; } = RunAs.CurrentUser;
    public StartMode StartMode { get; set; } = StartMode.Default;

    /// <summary>
    ///     Get or set a string representing all the name this alias has.
    ///     It should be a coma separated list of names.
    /// </summary>
    public string Synonyms
    {
        get => _synonyms;
        set
        {
            SetField(ref _synonyms, value);
            OnPropertyChanged(nameof(SynonymsToAdd));
        }
    }

    /// <summary>
    ///     New synonyms added when updated
    /// </summary>
    public string SynonymsToAdd => Synonyms.SplitCsv()
                                           .Where(n => !SynonymsWhenLoaded.SplitCsv().Contains(n))
                                           .ToArray()
                                           .JoinCsv();

    /// <summary>
    ///     Synonyms present when the entity was loaded
    /// </summary>
    public string SynonymsWhenLoaded { get; set; }

    public string WorkingDirectory { get; set; }

    #endregion

    #region Methods

    public static AliasQueryResult FromName(string aliasName) => new() { Name = aliasName, Synonyms = aliasName };

    #endregion
}