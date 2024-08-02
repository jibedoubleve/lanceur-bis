namespace Lanceur.Core;

[AttributeUsage(AttributeTargets.Class)]
public class ReservedAliasAttribute : Attribute
{
    #region Constructors

    public ReservedAliasAttribute(string name) => Name = name;

    #endregion Constructors

    #region Properties

    public string Name { get; set; }

    #endregion Properties
}