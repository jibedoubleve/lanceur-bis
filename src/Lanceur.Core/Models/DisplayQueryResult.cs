﻿namespace Lanceur.Core.Models;

public class DisplayQueryResult : QueryResult
{
    #region Fields

    private readonly string _description;
    private string _icon;

    #endregion Fields

    #region Constructors

    public DisplayQueryResult(string name, string description = null, string iconKind = null)
    {
        Name = name;
        _description = description;
        _icon = iconKind;
    }

    #endregion Constructors

    #region Properties

    public static IEnumerable<QueryResult> NoResultFound => SingleFromResult("No result found", iconKind: "AlertCircleOutline");

    public override string Description => _description;

    public override bool IsResult => false;

    public override string Icon
    {
        get => _icon;
        set => _icon = value;
    }

    #endregion Properties

    #region Methods

    public static IEnumerable<QueryResult> SingleFromResult(string text, string subtext = null, string iconKind = null) => new List<QueryResult> { new DisplayQueryResult(text, subtext, iconKind) };

    #endregion Methods
}