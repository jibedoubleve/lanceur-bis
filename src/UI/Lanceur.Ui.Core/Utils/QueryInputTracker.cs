using Lanceur.Core.Models;
using Lanceur.SharedKernel.Extensions;

namespace Lanceur.Ui.Core.Utils;

public sealed class QueryInputTracker
{
    #region Properties

    private Cmdline Previous { get; set; } = Cmdline.Empty;

    public Cmdline Current { get; private set; } = Cmdline.Empty;

    #endregion

    #region Methods

    public void Reset()
    {
        Previous = Cmdline.Empty;
        Current = Cmdline.Empty;
    }

    public TrackingReport Track(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Reset();
            return TrackingReport.Empty;
        }

        Previous = Current;

        if (IsDeletingTrailingWhitespace(query, Previous.Raw))
        {
            Current = Cmdline.Parse(query.TrimEnd());
            return TrackingReport.Deleting;
        }

        Current = Cmdline.Parse(query);

        var report = (Current, Previous) switch
        {
            var (c, p) when IsDeleting(c, p)              => TrackingReport.Deleting,
            var (c, p) when IsInParameters(c, p)          => TrackingReport.EnteringParameters,
            var (c, p) when c.Name.Length > p.Name.Length => TrackingReport.EnteringName,
            _                                             => TrackingReport.Other
        };

        return report;

        static bool IsDeleting(Cmdline current, Cmdline previous)
        {
            if (previous.Raw.Trim().IsNullOrEmpty()) { return false; }

            if (current == Cmdline.Empty) { return false; }

            return current.Raw.Length < previous.Raw.Length;
        }

        // if current is included in previous ("abc" is included in "abcd") they are the
        // same if they only differs by trailing white spaces
        static bool IsDeletingTrailingWhitespace(string current, string previous)
        {
            if (current.Length >= previous.Length) { return false; }

            if (!previous.StartsWith(current, StringComparison.Ordinal)) { return false; }

            return previous[current.Length..].IsNullOrWhiteSpace();
        }

        static bool IsInParameters(Cmdline current, Cmdline previous)
        {
            if (previous.Raw.Trim().IsNullOrEmpty()) { return false; }

            if (current.Raw.EndsWith(' ')) { return true; }

            return current.Parameters.Length > previous.Parameters.Length;
        }
    }

    #endregion
}

public enum TrackingReport
{
    EnteringName,
    EnteringParameters,
    Other,
    Deleting,
    Empty
}