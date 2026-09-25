using ProjectTemplate.Application.Inspection;
using ProjectTemplate.Infrastructure.Files;

namespace ProjectTemplate.Bootstrap;

/// <summary>Use cases available to the Desktop and CLI hosts.</summary>
public sealed class AppServices
{
    public required InspectFileUseCase InspectFile { get; init; }
}

/// <summary>
/// Wires the object graph by hand. Add a constructor call here for each new use case;
/// no DI container is needed until the graph is too large to read in one screen.
/// </summary>
public static class CompositionRoot
{
    public static AppServices Create()
    {
        var files = new FileSystemFileSource();
        return new AppServices
        {
            InspectFile = new InspectFileUseCase(files),
        };
    }
}
