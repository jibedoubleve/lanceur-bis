using System.IO.FileOps.Core;
using System.IO.FileOps.Core.Models;
using System.IO.FileOps.Infrastructure.Operations;
using System.Reflection;

namespace System.IO.FileOps.Infrastructure;

public static class OperationMixin
{
    private static readonly IEnumerable<Type> Types =
        Assembly.GetAssembly(typeof(AbstractOperation))?.GetTypes()
        ?? Type.EmptyTypes;
    
    public static IOperation AsOperation(this OperationConfiguration cfg)
    {
        var type =
            (from t in Types
             where t.FullName == cfg.Name
             select t).FirstOrDefault();

        if (type is null) throw new NotSupportedException($"Cannot find operation '{cfg.Name}'");

        return (IOperation)Activator.CreateInstance(type, cfg.Parameters)!;
    }
}