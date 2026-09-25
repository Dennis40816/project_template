using System.Security.Cryptography;
using ProjectTemplate.Application.Ports;
using ProjectTemplate.Application.Results;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Application.Inspection;

/// <summary>
/// Sample vertical slice: computes a file's fingerprint. Desktop and CLI both call this
/// one use case, so the result is computed on exactly one path.
/// </summary>
public sealed class InspectFileUseCase
{
    private const int BufferSize = 81_920;
    private readonly IFileSource _files;

    public InspectFileUseCase(IFileSource files)
    {
        ArgumentNullException.ThrowIfNull(files);
        _files = files;
    }

    public async Task<Result<FileFingerprint>> ExecuteAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Result.Failure<FileFingerprint>(new Issue("input.path-missing", "Choose a file to inspect."));
        }

        try
        {
            Stream stream = _files.OpenRead(path);
            await using (stream.ConfigureAwait(false))
            {
                return Result.Success(await FingerprintAsync(stream, cancellationToken).ConfigureAwait(false));
            }
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Result.Failure<FileFingerprint>(new Issue("input.file-not-found", $"File not found: {path}"));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Result.Failure<FileFingerprint>(new Issue("input.file-unreadable", exception.Message));
        }
    }

    private static async Task<FileFingerprint> FingerprintAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        byte[] buffer = new byte[BufferSize];
        long length = 0;
        int read;
        while ((read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            hash.AppendData(buffer, 0, read);
            length = checked(length + read);
        }

        return new FileFingerprint(length, Convert.ToHexStringLower(hash.GetHashAndReset()));
    }
}
