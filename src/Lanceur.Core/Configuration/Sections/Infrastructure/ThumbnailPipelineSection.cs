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
    ///     Accepted values: <c>"Wait"</c>, <c>"DropNewest"</c>, <c>"DropOldest"</c>, <c>"DropWrite"</c>.
    ///     Defaults to <c>"Wait"</c>.
    /// </summary>
    public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.Wait;

    /// <summary>
    ///     Gets or sets the number of consumer tasks reading from the channel
    ///     and processing thumbnail updates concurrently.
    /// </summary>
    public int MaxThreads { get; set; } = 10;

    #endregion
}