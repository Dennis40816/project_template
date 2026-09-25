using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace ProjectTemplate.Architecture.Tests;

/// <summary>
/// Structural rules only: project files and compiled assembly metadata.
/// These tests never match source text, so renaming or refactoring code does not break them.
/// To change a rule, change the table below in the same pull request as the ADR that justifies it.
/// </summary>
[Trait("Category", "architecture")]
public sealed class DependencyRulesTests
{
    private const string Prefix = "ProjectTemplate";

    /// <summary>Allowed direct project references, by project short name.</summary>
    private static readonly Dictionary<string, string[]> AllowedProjectReferences = new()
    {
        ["Domain"] = [],
        ["Application"] = ["Domain"],
        ["Infrastructure"] = ["Application"],
        ["Bootstrap"] = ["Application", "Infrastructure"],
        ["Desktop"] = ["Application", "Bootstrap"],
        ["Cli"] = ["Application", "Bootstrap"],
    };

    /// <summary>Product assemblies each assembly may use in compiled code. Desktop and CLI never touch Infrastructure.</summary>
    private static readonly Dictionary<string, string[]> AllowedCompiledReferences = new()
    {
        ["Domain"] = [],
        ["Application"] = ["Domain"],
        ["Infrastructure"] = ["Application", "Domain"],
        ["Bootstrap"] = ["Application", "Domain", "Infrastructure"],
        ["Desktop"] = ["Application", "Bootstrap", "Domain"],
        ["Cli"] = ["Application", "Bootstrap", "Domain"],
    };

    public static TheoryData<string> Projects() => new(AllowedProjectReferences.Keys);

    [Fact]
    public void EverySourceProjectHasARule()
    {
        string[] projects = Directory.GetFiles(Path.Combine(RepositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Select(path => ShortName(Path.GetFileNameWithoutExtension(path)))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(AllowedProjectReferences.Keys.Order(StringComparer.Ordinal), projects);
    }

    [Theory]
    [MemberData(nameof(Projects))]
    public void ProjectReferencesMatchTheAllowList(string project)
    {
        string csproj = Path.Combine(RepositoryRoot, "src", $"{Prefix}.{project}", $"{Prefix}.{project}.csproj");
        string[] actual = XDocument.Load(csproj)
            .Descendants("ProjectReference")
            .Select(reference => ShortName(Path.GetFileNameWithoutExtension((string)reference.Attribute("Include")!)))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(AllowedProjectReferences[project].Order(StringComparer.Ordinal), actual);
    }

    [Theory]
    [MemberData(nameof(Projects))]
    public void CompiledReferencesStayInsideTheAllowList(string project)
    {
        string[] used = Load(project).GetReferencedAssemblies()
            .Select(name => name.Name!)
            .Where(name => name.StartsWith(Prefix + ".", StringComparison.Ordinal))
            .Select(ShortName)
            .ToArray();

        string[] forbidden = used.Except(AllowedCompiledReferences[project]).ToArray();
        Assert.True(forbidden.Length == 0, $"{project} uses forbidden assemblies: {string.Join(", ", forbidden)}");
    }

    [Theory]
    [MemberData(nameof(Projects))]
    public void InternalsAreVisibleOnlyToTests(string project)
    {
        string[] friends = Load(project).GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName)
            .Where(name => !name.EndsWith(".Tests", StringComparison.Ordinal))
            .ToArray();

        Assert.True(friends.Length == 0, $"{project} exposes internals to non-test assemblies: {string.Join(", ", friends)}");
    }

    private static Assembly Load(string project) => Assembly.Load(new AssemblyName($"{Prefix}.{project}"));

    private static string ShortName(string assemblyName) =>
        assemblyName.StartsWith(Prefix + ".", StringComparison.Ordinal) ? assemblyName[(Prefix.Length + 1)..] : assemblyName;

    private static string RepositoryRoot { get; } = FindRepositoryRoot();

    private static string FindRepositoryRoot()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Directory.Build.props")) && Directory.Exists(Path.Combine(dir.FullName, "src")))
            {
                return dir.FullName;
            }
        }

        throw new DirectoryNotFoundException("Repository root was not found above the test output directory.");
    }
}
