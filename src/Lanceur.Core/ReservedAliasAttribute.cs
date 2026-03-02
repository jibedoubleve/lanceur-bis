namespace Lanceur.Core;

[AttributeUsage(AttributeTargets.Class)]
public class ReservedAliasAttribute : Attribute
{
    #region Constructors

    public ReservedAliasAttribute(string name) => Name = name;

    #endregion

    #region Properties

    public string Name { get; set; }

    #endregion
}