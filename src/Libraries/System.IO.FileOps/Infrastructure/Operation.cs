using System.IO.FileOps.Infrastructure.Operations;

namespace System.IO.FileOps.Infrastructure;

public struct OperationInfo
{
    public string Name { get; }
    public string Key { get; }
    public string Value { get; }

    private OperationInfo(string operation, string key, string value)
    {
        Name = operation;
        Key           = key;
        Value         = value;
    }

    public static OperationInfo UnZip(string key, string value) => new(typeof(UnzipOperation).FullName, key, value);
}