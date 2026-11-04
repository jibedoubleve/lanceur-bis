using Lanceur.Ui.Core.Utils;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.Ui;

public sealed class QueryInputTrackerTest
{
    #region Methods

    [Theory]
    [InlineData("note    ", "note   ", "note")]
    [InlineData("note ", "note", "note")]
    [InlineData("note    world ", "note    world", "note    world")]
    [InlineData("  note    ", "  note   ", "  note")]
    public void When_backspacing_on_whitespace_Then_all_trailing_whitespace_is_removed(
        string previous,
        string current,
        string expected
    )
    {
        var sut = new QueryInputTracker();
        sut.Track(previous);

        var report = sut.Track(current);

        report.ShouldBe(TrackingReport.Deleting);
        sut.Current.Raw.ShouldBe(expected);
    }

    [Fact]
    public void When_backspacing_on_a_letter_Then_trailing_whitespace_is_kept()
    {
        var sut = new QueryInputTracker();
        sut.Track("note    w");

        sut.Track("note    ");

        sut.Current.Raw.ShouldBe("note    ");
    }

    #endregion
}
