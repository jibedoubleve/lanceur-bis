using FluentAssertions;
using Lanceur.SharedKernel.Mixins;
using Xunit;

namespace Lanceur.Tests.SharedKernel;

public class IndexNavigatorShould
{
    #region Methods

    [Fact]
    public void GetAlwaysTheSameWhenOneItemListWhenNext()
    {
        var list = new List<string>() { "un" };
        list.GetNextItem(0).Should().Be("un");
    }

    [Fact]
    public void GetAlwaysTheSameWhenOneItemListWhenPrevious()
    {
        var list = new List<string>() { "un" };
        list.GetPreviousItem(0).Should().Be("un");
    }

    [Fact]
    public void GetErrorIndexWhenCurrentIsMinusOneOnNextInEmptyCollection()
    {
        var list = new List<string>();
        var action = () => list.GetNextItem(-1);

        action.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void GetErrorIndexWhenCurrentIsMinusOneOnPreviousInEmptyCollection()
    {
        var list = new List<string>();
        var action = () => list.GetPreviousItem(-1);

        action.Should().Throw<IndexOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 5)]
    [InlineData(5, 0)]
    public void GetNextIndex(int next, int expected)
    {
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };

        list[list.GetNextIndex(next)].Should().Be(expected);
    }

    [Fact]
    public void GetNextIndexWithEmptyList()
    {
        var list = new List<int>() { };

        var subject = () => list[list.GetNextIndex(0)];
        subject.Should().Throw<IndexOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, "un")]
    [InlineData(1, "deux")]
    [InlineData(2, "trois")]
    [InlineData(3, "quatre")]
    [InlineData(4, "cinq")]
    [InlineData(5, "zero")]
    public void GetNextItem(int next, string expected)
    {
        var list = new List<string>() { "zero", "un", "deux", "trois", "quatre", "cinq" };

        list.GetNextItem(next).Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 4)]
    [InlineData(4, 3)]
    [InlineData(3, 2)]
    [InlineData(2, 1)]
    [InlineData(1, 0)]
    [InlineData(0, 5)]
    public void GetPreviousIndex(int next, int expected)
    {
        var list = new List<int>() { 0, 1, 2, 3, 4, 5 };

        list[list.GetPreviousIndex(next)].Should().Be(expected);
    }

    [Fact]
    public void GetPreviousIndexWithEmptyList()
    {
        var list = new List<int>() { };

        var subject = () => list[list.GetPreviousIndex(0)];
        subject.Should().Throw<IndexOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, "cinq")]
    [InlineData(1, "zero")]
    [InlineData(2, "un")]
    [InlineData(3, "deux")]
    [InlineData(4, "trois")]
    [InlineData(5, "quatre")]
    public void GetPreviousItem(int next, string expected)
    {
        var list = new List<string>() { "zero", "un", "deux", "trois", "quatre", "cinq" };

        list.GetPreviousItem(next).Should().Be(expected);
    }

    [Fact]
    public void GetZeroIndexWhenCurrentIsMinusOneWhenNext()
    {
        var list = new List<string>() { "un" };
        list.GetNextItem(-1).Should().Be("un");
    }

    [Fact]
    public void GetZeroIndexWhenCurrentIsMinusOneWhenPrevious()
    {
        var list = new List<string>() { "un" };
        list.GetPreviousItem(-1).Should().Be("un");
    }

    [Fact]
    public void IndicateNavigationImpossibleWithList()
    {
        var list = new List<int>();
        list.CanNavigate().Should().Be(false);
    }

    [Fact]
    public void IndicateNavigationPossibleWithList()
    {
        var list = new List<int>() { 1, 2, 3 };
        list.CanNavigate().Should().Be(true);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(1)]
    public void ThrowsWhenIndexBiggerThanCountOnNext(int index)
    {
        var list = new List<string>() { "un" };
        var subject = () => list.GetPreviousItem(index);
        subject.Should().Throw<IndexOutOfRangeException>();
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(1)]
    public void ThrowsWhenIndexBiggerThanCountOnPrevious(int index)
    {
        var list = new List<string>() { "un" };
        var subject = () => list.GetNextItem(index);
        subject.Should().Throw<IndexOutOfRangeException>();
    }

    #endregion Methods
}