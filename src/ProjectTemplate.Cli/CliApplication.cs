using System.Reflection;
using System.Text.Json;
using ProjectTemplate.Application.Results;
using ProjectTemplate.Bootstrap;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Cli;

/// <summary>
/// Command-line host. Every failure ends in a controlled exit code; the process never
/// ends with an unhandled exception for bad input.
/// </summary>
public static class CliApplication
{
    public const int ExitOk = 0;
    public const int ExitIssues = 1;
    public const int ExitUsage = 2;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, AppServices services, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        switch (args)
        {
            case ["--version"]:
                await output.WriteLineAsync(InformationalVersion).ConfigureAwait(false);
                return ExitOk;
            case ["inspect", string path]:
                Result<FileFingerprint> result = await services.InspectFile.ExecuteAsync(path).ConfigureAwait(false);
                if (result.IsSuccess && result.Value is { } fingerprint)
                {
                    await output.WriteLineAsync(JsonSerializer.Serialize(
                        new { length = fingerprint.Length, sha256 = fingerprint.Sha256 },
                        JsonOptions)).ConfigureAwait(false);
                    return ExitOk;
                }

                foreach (Issue issue in result.Issues)
                {
                    await error.WriteLineAsync($"{issue.Code}: {issue.Message}").ConfigureAwait(false);
                }

                return ExitIssues;
            default:
                await error.WriteLineAsync("Usage: ProjectTemplate.Cli inspect <path> | --version").ConfigureAwait(false);
                return ExitUsage;
        }
    }

    private static string InformationalVersion =>
        typeof(CliApplication).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";
}
