using ProjectTemplate.Application.Inspection;
using ProjectTemplate.Application.Ports;

namespace ProjectTemplate.Application.Tests;

[Trait("Category", "unit")]
public sealed class InspectFileUseCaseTests
{
    [Fact]
    public async Task ComputesLengthAndSha256()
    {
        var useCase = new InspectFileUseCase(new InMemoryFileSource("a.bin", "abc"u8.ToArray()));

        var result = await useCase.ExecuteAsync("a.bin", TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Length);
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", result.Value.Sha256);
    }

    [Fact]
    public async Task MissingFileIsAnIssueNotAnException()
    {
        var useCase = new InspectFileUseCase(new InMemoryFileSource("a.bin", []));

        var result = await useCase.ExecuteAsync("missing.bin", TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("input.file-not-found", Assert.Single(result.Issues).Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BlankPathIsAnIssue(string path)
    {
        var useCase = new InspectFileUseCase(new InMemoryFileSource("a.bin", []));

        var result = await useCase.ExecuteAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal("input.path-missing", Assert.Single(result.Issues).Code);
    }

    private sealed class InMemoryFileSource(string path, byte[] content) : IFileSource
    {
        public Stream OpenRead(string requestedPath) =>
            requestedPath == path
                ? new MemoryStream(content, writable: false)
                : throw new FileNotFoundException("Not found.", requestedPath);
    }
}
