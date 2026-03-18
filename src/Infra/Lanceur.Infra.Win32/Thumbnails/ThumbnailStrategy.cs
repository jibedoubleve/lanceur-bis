using System.IO;
using Lanceur.Core.Models;
using Lanceur.Infra.Win32.Extensions;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Thumbnails;

public abstract class ThumbnailStrategy : IThumbnailStrategy
{
    #region Fields

    private readonly ILogger _logger;

    #endregion

    #region Constructors

    protected ThumbnailStrategy(ILogger logger) => _logger = logger;

    #endregion

    #region Properties

    /// <inheritdoc />
    public virtual int Priority { get; } = int.MaxValue;

    #endregion

    #region Methods

    /// <summary>
    ///     Fetches and sets the thumbnail for the specified alias.
    /// </summary>
    /// <remarks>
    ///     This method is called by <see cref="UpdateThumbnailAsync" /> only when the thumbnail
    ///     is not already cached and the alias has a valid file name. Implementations do not
    ///     need to repeat these checks.
    /// </remarks>
    /// <param name="alias">The alias for which to update the thumbnail.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    protected abstract Task UpdateThumbnailCoreAsync(AliasQueryResult alias, CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task UpdateThumbnailAsync(AliasQueryResult alias, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Executing thumbnail strategy {Strategy} for alias {Alias}",
            GetType().FullName,
            alias.Name
        );

        if (File.Exists(alias.Thumbnail))
        {
            _logger.LogTrace(
                "Thumbnail for alias {Name} found in cache at {Path}.",
                alias.Name,
                alias.Thumbnail
            );
            return;
        }

        if (ResolveCachePathExists(out var path))
        {
            _logger.LogTrace(
                "Thumbnail for alias {Name} found in cache at {Path}. Alias thumbnail path was missing and has been updated.",
                alias.Name,
                path
            );
            alias.Thumbnail ??=  path;
            return;
        }

        if (alias.FileName is null)
        {
            _logger.LogInformation("Alias {Alias} does not have a file name.", alias.Name);
            return;
        }

        await UpdateThumbnailCoreAsync(alias, cancellationToken);

        return;

        bool ResolveCachePathExists(out string path)
        {
            path = alias.ResolveThumbnailAbsolutePath();
            var pathExits = File.Exists(path);
            _logger.LogTrace("Thumbnail strategy: Resolved cache path {Path} {Exists}.",
                path,
                pathExits ? "exists" : "does not exist"
            );
            return pathExits;
        }
    }

    #endregion
}