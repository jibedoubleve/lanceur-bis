using System.IO.FileOps.Infrastructure.Operations;

namespace System.IO.FileOps.Infrastructure;

public struct OperationInfo
{
    #region Constructors

    private OperationInfo(string operation, string key, string value)
    {
        Name = operation;
        Key = key;
        Value = value;
    }

    #endregion Constructors

    #region Properties

    public string Key { get; }
    public string Name { get; }
    public string Value { get; }

    #endregion Properties

    #region Methods

    public static OperationInfo UnZip(string key, string value) =>
        new(typeof(UnzipOperation)?.FullName ?? "", key, value);

    #endregion Methods
}