using Lanceur.SharedKernel.Extensions;
using Shouldly;
using Xunit;

namespace Lanceur.Tests.SharedKernel;

public sealed class IndexNavigatorTest
{
    #region Methods

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 5)]
    [InlineData(5, 0)]
    public void When_getting_next_index_Then_it_wraps_at_the_end(int current, int next)
    {
        var list = new List<int>
        {
            0,
            1,
            2,
            3,
            4,
            5
        };

        list[list.GetNextIndex(current)].ShouldBe(next);
    }

    [Theory]
    [InlineData(0, "un")]
    [InlineData(1, "deux")]
    [InlineData(2, "trois")]
    [InlineData(3, "quatre")]
    [InlineData(4, "cinq")]
    [InlineData(5, "zero")]
    public void When_getting_next_item_Then_it_wraps_at_the_end(int next, string expected)
    {
        var list = new List<string>
        {
            "zero",
            "un",
            "deux",
            "trois",
            "quatre",
            "cinq"
        };

        list.GetNextItem(next).ShouldBe(expected);
    }

    [Fact]
    public void When_getting_next_item_of_empty_list_Then_it_throws()
    {
        var list = new List<string>();
        var action = () => list.GetNextItem(-1);

        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void When_getting_next_item_of_single_item_list_Then_same_item_is_returned()
    {
        var list = new List<string> { "un" };
        list.GetNextItem(0).ShouldBe("un");
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(1)]
    public void When_getting_next_item_with_out_of_range_index_Then_it_throws(int index)
    {
        var list = new List<string> { "un" };
        var subject = () => list.GetNextItem(index);
        subject.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 3)]
    [InlineData(1, 4)]
    [InlineData(2, 5)]
    [InlineData(3, 6)]
    [InlineData(4, 7)]
    [InlineData(5, 0)]
    [InlineData(6, 0)]
    [InlineData(7, 0)]
    public void When_getting_next_page_Then_expected_index_is_returned(int current, int next)
    {
        var list = new List<int>
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7
        };

        list.GetNextPage(current, 3)
            .ShouldBe(next);
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(4, 3)]
    [InlineData(3, 2)]
    [InlineData(2, 1)]
    [InlineData(1, 0)]
    [InlineData(0, 5)]
    public void When_getting_previous_index_Then_it_wraps_at_the_start(int current, int previous)
    {
        var list = new List<int>
        {
            0,
            1,
            2,
            3,
            4,
            5
        };

        list[list.GetPreviousIndex(current)].ShouldBe(previous);
    }

    [Theory]
    [InlineData(0, "cinq")]
    [InlineData(1, "zero")]
    [InlineData(2, "un")]
    [InlineData(3, "deux")]
    [InlineData(4, "trois")]
    [InlineData(5, "quatre")]
    public void When_getting_previous_item_Then_it_wraps_at_the_start(int next, string expected)
    {
        var list = new List<string>
        {
            "zero",
            "un",
            "deux",
            "trois",
            "quatre",
            "cinq"
        };

        list.GetPreviousItem(next).ShouldBe(expected);
    }

    [Fact]
    public void When_getting_previous_item_of_empty_list_Then_it_throws()
    {
        var list = new List<string>();
        var action = () => list.GetPreviousItem(-1);

        action.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void When_getting_previous_item_of_single_item_list_Then_same_item_is_returned()
    {
        var list = new List<string> { "un" };
        list.GetPreviousItem(0).ShouldBe("un");
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(1)]
    public void When_getting_previous_item_with_out_of_range_index_Then_it_throws(int index)
    {
        var list = new List<string> { "un" };
        var subject = () => list.GetPreviousItem(index);
        subject.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 7)]
    [InlineData(1, 7)]
    [InlineData(2, 7)]
    [InlineData(3, 0)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    [InlineData(6, 3)]
    [InlineData(7, 4)]
    public void When_getting_previous_page_Then_expected_index_is_returned(int current, int previous)
    {
        var list = new List<int>
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7
        };

        list.GetPreviousPage(current, 3)
            .ShouldBe(previous);
    }

    [Fact]
    public void When_indexing_empty_list_with_next_index_Then_it_throws()
    {
        var list = new List<int>();

        Should.Throw<ArgumentOutOfRangeException>(() => list[list.GetNextIndex(0)]);
    }

    [Fact]
    public void When_indexing_empty_list_with_previous_index_Then_it_throws()
    {
        var list = new List<int>();
        Should.Throw<ArgumentOutOfRangeException>(() => list[list.GetPreviousIndex(0)]
        );
    }

    [Fact]
    public void When_list_has_items_Then_navigation_is_possible()
    {
        var list = new List<int> { 1, 2, 3 };
        list.CanNavigate().ShouldBe(true);
    }

    [Fact]
    public void When_list_is_empty_Then_navigation_is_impossible()
    {
        var list = new List<int>();
        list.CanNavigate().ShouldBe(false);
    }

    #endregion
}