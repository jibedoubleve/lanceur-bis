using System.Threading.Channels;

namespace Lanceur.Core.Configuration.Sections.Infrastructure;

/// <summary>
///     Configuration section for the thumbnail update pipeline.
///     Controls the behaviour of the bounded channel used to queue thumbnail requests.
/// </summary>
public class ThumbnailPipelineSection
{
    #region Properties

    /// <summary>
    ///     Gets or sets the maximum number of thumbnail requests that can be queued
    ///     in the bounded channel at any given time.
    ///     Defaults to <c>100</c>.
    /// </summary>
    public int ChannelCapacity { get; set; } = 100;

    /// <summary>
    ///     Gets or sets the behaviour when the channel has reached its maximum capacity.
    ///     Maps to <see cref="System.Threading.Channels.BoundedChannelFullMode" />.
    ///     Accepted values: <c>"DropNewest"</c>, <c>"DropOldest"</c>, <c>"DropWrite"</c>.
    ///     Defaults to <c>"DropOldest"</c>.
    /// </summary>
    /// <remarks>
    ///     <c>"Wait"</c> is not supported: the pipeline uses <c>TryWrite</c>, which
    ///     never blocks. When the channel is full, the item is silently dropped regardless of this setting.
    /// </remarks>
    public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.DropOldest;

    /// <summary>
    ///     Gets or sets the number of consumer tasks reading from the channel
    ///     and processing thumbnail updates concurrently.
    /// </summary>
    public int ConsumerCount { get; set; } = 4;

    #endregion
}