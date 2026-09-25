using System.Reflection;
using ProjectTemplate.Desktop.Features.Inspect;

namespace ProjectTemplate.Desktop.Shell;

public sealed class MainWindowViewModel
{
    public MainWindowViewModel(InspectViewModel inspect)
    {
        ArgumentNullException.ThrowIfNull(inspect);
        Inspect = inspect;
    }

    public InspectViewModel Inspect { get; }

    public string Title { get; } =
        $"ProjectTemplate {typeof(MainWindowViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}";
}
