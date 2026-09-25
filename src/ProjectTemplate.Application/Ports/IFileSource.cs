namespace ProjectTemplate.Application.Ports;

/// <summary>Read access to files. Implemented in Infrastructure; faked in tests.</summary>
public interface IFileSource
{
    /// <summary>Opens a file for sequential reading.</summary>
    /// <exception cref="IOException">The file cannot be read.</exception>
    /// <exception cref="UnauthorizedAccessException">Access is denied.</exception>
    Stream OpenRead(string path);
}
