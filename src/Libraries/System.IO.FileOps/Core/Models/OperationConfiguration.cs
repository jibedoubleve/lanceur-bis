namespace System.IO.FileOps.Core.Models;

public class OperationConfiguration : IEquatable<OperationConfiguration>
{
    #region Properties

    public string? Name { get; init; }
    public Dictionary<string, string> Parameters { get; set; } = new();

    #endregion Properties

    #region Methods

    public static bool operator !=(OperationConfiguration left, OperationConfiguration right) => !(left == right);

    public static bool operator ==(OperationConfiguration left, OperationConfiguration right) => left.Equals(right);

    public bool Equals(OperationConfiguration? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        // https://stackoverflow.com/a/9547410/389529
        var areEqual = Parameters.OrderBy(kvp => kvp.Key)
                                 .SequenceEqual(other.Parameters.OrderBy(kvp => kvp.Key));

        return Name == other.Name && areEqual;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;

        return obj.GetType() == GetType() && Equals((OperationConfiguration)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Name, Parameters);

    #endregion Methods
}