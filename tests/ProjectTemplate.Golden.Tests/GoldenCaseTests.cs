using System.Security.Cryptography;
using System.Text.Json;
using ProjectTemplate.Bootstrap;
using ProjectTemplate.Cli;

namespace ProjectTemplate.Golden.Tests;

/// <summary>
/// Runs every case in testdata/golden/manifest.json through the real host and compares the
/// output exactly. A failing case is a behavior change: fix the code, or get the owner to
/// approve a new expected file and record that approval in the manifest.
/// </summary>
[Trait("Category", "golden")]
public sealed class GoldenCaseTests
{
    public static TheoryData<string> CaseIds() => new(Manifest.Load().Cases.Select(c => c.Id));

    [Theory]
    [MemberData(nameof(CaseIds))]
    public async Task OutputMatchesApprovedGolden(string caseId)
    {
        GoldenCase goldenCase = Manifest.Load().Cases.Single(c => c.Id == caseId);
        string input = Manifest.Resolve(goldenCase.Input);
        Assert.Equal(goldenCase.InputSha256, Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(input))));

        string[] args = goldenCase.Command.Select(a => a.Replace("{input}", input, StringComparison.Ordinal)).ToArray();
        using var output = new StringWriter();
        using var error = new StringWriter();
        int exitCode = await CliApplication.RunAsync(args, CompositionRoot.Create(), output, error);

        Assert.Equal(CliApplication.ExitOk, exitCode);
        Assert.Equal(Normalize(File.ReadAllText(Manifest.Resolve(goldenCase.Expected))), Normalize(output.ToString()));
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd();

    private sealed record GoldenCase(string Id, string[] Command, string Input, string InputSha256, string Expected);

    private sealed record Manifest(GoldenCase[] Cases)
    {
        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static string Root { get; } = FindGoldenRoot();

        public static Manifest Load() =>
            JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Path.Combine(Root, "manifest.json")), Options)
            ?? throw new InvalidOperationException("Golden manifest is empty.");

        public static string Resolve(string relativePath) => Path.GetFullPath(Path.Combine(Root, relativePath));

        private static string FindGoldenRoot()
        {
            for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, "testdata", "golden");
                if (File.Exists(Path.Combine(candidate, "manifest.json")))
                {
                    return candidate;
                }
            }

            throw new DirectoryNotFoundException("testdata/golden/manifest.json was not found above the test output directory.");
        }
    }
}
