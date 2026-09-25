using ProjectTemplate.Application.Ports;

namespace ProjectTemplate.Infrastructure.Files;

/// <summary>Reads files from the local file system.</summary>
public sealed class FileSystemFileSource : IFileSource
{
    private const int BufferSize = 81_920;

    public Stream OpenRead(string path) =>
        new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
}
