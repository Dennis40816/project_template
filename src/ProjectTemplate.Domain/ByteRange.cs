namespace ProjectTemplate.Domain;

/// <summary>
/// A half-open byte range <c>[Start, EndExclusive)</c>. Every range in the product uses this
/// convention; arithmetic is checked so an overflow fails instead of wrapping.
/// </summary>
public readonly record struct ByteRange
{
    public ByteRange(long start, long endExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfLessThan(endExclusive, start);
        Start = start;
        EndExclusive = endExclusive;
    }

    public long Start { get; }

    public long EndExclusive { get; }

    public long Length => EndExclusive - Start;

    public bool IsEmpty => Length == 0;

    public static ByteRange FromStartAndLength(long start, long length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        return new ByteRange(start, checked(start + length));
    }

    public bool Contains(long offset) => offset >= Start && offset < EndExclusive;

    public bool Contains(ByteRange other) => other.Start >= Start && other.EndExclusive <= EndExclusive;

    /// <summary>An empty range overlaps nothing.</summary>
    public bool Overlaps(ByteRange other) =>
        !IsEmpty && !other.IsEmpty && Start < other.EndExclusive && other.Start < EndExclusive;

    public override string ToString() => $"[0x{Start:X}, 0x{EndExclusive:X})";
}
