using System.IO;
using System.Threading.Channels;
using ABI.Windows.ApplicationModel.Contacts;
using Lanceur.Core.Configuration;
using Lanceur.Core.Configuration.Sections.Infrastructure;
using Lanceur.Core.Models;
using Lanceur.Core.Services;
using Lanceur.Infra.Win32.Thumbnails;
using Lanceur.SharedKernel.Extensions;
using Lanceur.SharedKernel.Logging;
using Microsoft.Extensions.Logging;

namespace Lanceur.Infra.Win32.Services;

public sealed class ThumbnailService : IThumbnailService, IAsyncDisposable
{
    #region Fields

    private readonly Channel<AliasQueryResult> _channel;
    private readonly Task[] _consumers;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<ThumbnailService> _logger;
    private readonly IEnumerable<IThumbnailStrategy> _thumbnailStrategy;

    #endregion

    #region Constructors

    public ThumbnailService(
        ILoggerFactory loggerFactory,
        IEnumerable<IThumbnailStrategy> thumbnailStrategy,
        ISection<ThumbnailPipelineSection> section
    )
    {
        _thumbnailStrategy = thumbnailStrategy;
        _logger = loggerFactory.GetLogger<ThumbnailService>();

        var s = section.Value;
        _channel = Channel.CreateBounded<AliasQueryResult>(
            new BoundedChannelOptions(s.ChannelCapacity) { FullMode = s.ChannelFullMode }
        );

        _consumers = Enumerable.Range(0, s.ConsumerCount)
                               .Select(_ => Task.Run(ConsumeAsync))
                               .ToArray();
    }

    #endregion

    #region Methods

    private static bool CanReturnEarly(QueryResult queryResult)
    {
        if (queryResult.IsThumbnailDisabled) { return true; }

        if (queryResult is not AliasQueryResult alias) { return true; }

        if (File.Exists(alias.Thumbnail)) { return true; }

        if (alias.FileName.IsNullOrEmpty()) { return true; }

        return false;
    }

    private async Task ConsumeAsync()
    {
        try
        {
            await foreach (var alias in _channel.Reader.ReadAllAsync(_cts.Token))
            {
                await RunStrategy(alias);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Cancellation requested during thumbnail consume");
        }

        return;

        async Task RunStrategy(AliasQueryResult alias)
        {
            foreach (var strategy in _thumbnailStrategy)
            {
                try { await strategy.UpdateThumbnailAsync(alias, _cts.Token); }
                catch (OperationCanceledException ex)
                {
                    _logger.LogDebug(ex, "Cancellation requested during thumbnail update of {AliasName}", alias.Name);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "One thumbnail retrieve strategy failed");
                }
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _channel.Writer.TryComplete();
        await Task.WhenAll(_consumers);
        _cts.Dispose();
    }


    /// <summary>
    ///     Starts a thread to refresh the thumbnails asynchronously. This method returns immediately after starting the
    ///     thread.
    ///     Each time a thumbnail is found, the corresponding alias is updated. Because the alias is reactive, the UI will
    ///     automatically reflect these updates.
    /// </summary>
    /// <param name="queryResult">The list of queries for which thumbnails need to be updated.</param>
    public void UpdateThumbnail(QueryResult queryResult)
    {
        if (CanReturnEarly(queryResult)) { return; }

        _logger.LogTrace("Loading thumbnail for {AliasName}...", queryResult.Name);

        var aqr = (AliasQueryResult)queryResult;
        var written = _channel.Writer.TryWrite(aqr);
        if (!written)
        {
            _logger.LogInformation("Cannot add alias {AliasName} in the thumbnail pipeline", queryResult.Name);
        }
    }

    #endregion
}