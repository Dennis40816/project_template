namespace ProjectTemplate.Domain;

/// <summary>The identity of a file's content: its length and lowercase SHA-256.</summary>
public sealed record FileFingerprint
{
    private const int Sha256HexLength = 64;

    public FileFingerprint(long length, string sha256)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentNullException.ThrowIfNull(sha256);
        if (sha256.Length != Sha256HexLength || !sha256.All(IsLowerHex))
        {
            throw new ArgumentException("SHA-256 must be 64 lowercase hexadecimal characters.", nameof(sha256));
        }

        Length = length;
        Sha256 = sha256;
    }

    public long Length { get; }

    public string Sha256 { get; }

    private static bool IsLowerHex(char c) => c is (>= '0' and <= '9') or (>= 'a' and <= 'f');
}
