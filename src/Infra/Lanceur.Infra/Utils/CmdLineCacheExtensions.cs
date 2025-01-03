using Lanceur.Core.Models;

namespace Lanceur.Infra.Utils;

internal static class CmdLineCacheExtensions
{
    #region Fields

    private static readonly Guid UniqueKey = Guid.NewGuid();

    #endregion

    #region Methods

    /// <summary>
    /// Generates a unique cache key for the specified command-line object.
    /// The cache key combines a static unique identifier (GUID) with the name of the command-line instance,
    /// ensuring the key is both globally unique and meaningful within the context of the application.
    /// </summary>
    /// <param name="cmdline">
    /// The instance of the <see cref="Cmdline"/> class for which the cache key is being generated.
    /// This parameter must have a valid <see cref="Cmdline.Name"/> property to be appended to the key.
    /// </param>
    /// <returns>
    /// A string representing the unique cache key in the format: 
    /// "{UniqueKey}-{Cmdline.Name}" where <c>UniqueKey</c> is a predefined static GUID.
    /// </returns>
    public static string GetCacheKey(this Cmdline cmdline) => $"{UniqueKey}-{cmdline.Name}";


    #endregion
}