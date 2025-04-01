using FluentAssertions;
using Lanceur.SharedKernel.Utils;
using Xunit;

namespace Lanceur.Tests.SharedKernel;

public class CircularQueueShould
{
    #region Methods

    [Fact]
    public void GetAverage()
    {
        var values = new Dictionary<float, float>
        {
            [10] = 10f,
            [5] = 7.5f,
            [15] = 10f,
            [20] = 17.5f,
            [4] = 12f
        };
        var sut = new CircularQueue<float>(2);

        foreach (var value in values)
        {
            sut.Enqueue(value.Key);
            sut.Average().Should().Be(value.Value);
        }
    }

    [Fact]
    public void NeverHaveCountGreaterThanMax()
    {
        const int max = 2;
        var sut = new CircularQueue<float>(max);

        for (var i = 0; i < 10; i++)
        {
            sut.Enqueue(i);
            sut.Count.Should().BeLessOrEqualTo(max);
        }
    }

    [Fact]
    public void ResetQueue()
    {
        var sut = new CircularQueue<float>(2);
        sut.Enqueue(1);
        sut.Enqueue(2);

        sut.Count.Should().Be(2);

        sut.Clear();
        sut.Count.Should().Be(0);
    }

    #endregion
}