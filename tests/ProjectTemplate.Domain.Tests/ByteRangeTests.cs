using ProjectTemplate.Domain;

namespace ProjectTemplate.Domain.Tests;

[Trait("Category", "unit")]
public sealed class ByteRangeTests
{
    [Fact]
    public void RangeIsHalfOpen()
    {
        var range = new ByteRange(0x10, 0x20);

        Assert.Equal(0x10, range.Length);
        Assert.True(range.Contains(0x10));
        Assert.True(range.Contains(0x1F));
        Assert.False(range.Contains(0x20));
    }

    [Theory]
    [InlineData(0, 10, 10, 20, false)]
    [InlineData(0, 10, 9, 20, true)]
    [InlineData(5, 5, 0, 10, false)]
    public void OverlapUsesHalfOpenBounds(long aStart, long aEnd, long bStart, long bEnd, bool expected)
    {
        Assert.Equal(expected, new ByteRange(aStart, aEnd).Overlaps(new ByteRange(bStart, bEnd)));
    }

    [Fact]
    public void RejectsInvertedRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ByteRange(10, 9));
    }

    [Fact]
    public void LengthArithmeticIsChecked()
    {
        Assert.Throws<OverflowException>(() => ByteRange.FromStartAndLength(long.MaxValue, 1));
    }
}
