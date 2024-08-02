using System.IO.FileOps.Core;
using System.IO.FileOps.Core.Models;
using System.IO.FileOps.Infrastructure.Operations;
using System.Reflection;

namespace System.IO.FileOps.Infrastructure;

public static class OperationMixin
{
    #region Fields

    private static readonly IEnumerable<Type> Types =
        Assembly.GetAssembly(typeof(AbstractOperation))?.GetTypes() ?? Type.EmptyTypes;

    #endregion Fields

    #region Methods

    public static IOperation ToOperation(this OperationConfiguration cfg)
    {
        var type = Types.FirstOrDefault(t => t.FullName == cfg.Name);

        return type is null
            ? throw new NotSupportedException($"Cannot find operation '{cfg.Name}'")
            : (IOperation)Activator.CreateInstance(type, cfg.Parameters)!;
    }

    #endregion Methods
}