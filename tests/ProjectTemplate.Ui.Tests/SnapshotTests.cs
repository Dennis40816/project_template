using System.Runtime.CompilerServices;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using ProjectTemplate.Bootstrap;
using ProjectTemplate.Desktop;
using ProjectTemplate.Desktop.Shell;

namespace ProjectTemplate.Ui.Tests;

/// <summary>
/// Pixel-exact snapshots define the visual contract before features pile up.
/// A missing approved image skips the test and writes a candidate; approve it with
/// <c>scripts/approve-snapshots.ps1</c> after looking at it.
/// </summary>
[Trait("Category", "ui")]
public sealed class SnapshotTests
{
    [AvaloniaFact]
    public void MainWindowMatchesApprovedSnapshot()
    {
        MainWindow window = App.CreateMainWindow(CompositionRoot.Create());
        window.Show();

        using var frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The headless renderer produced no frame.");
        window.Close();

        string directory = SnapshotDirectory();
        string approved = Path.Combine(directory, "main-window.approved.png");
        string received = Path.Combine(directory, "main-window.received.png");
        Directory.CreateDirectory(directory);
        frame.Save(received);

        if (!File.Exists(approved))
        {
            Assert.Skip($"No approved snapshot yet. Review {received} and run scripts/approve-snapshots.ps1.");
        }

        Assert.True(
            File.ReadAllBytes(approved).AsSpan().SequenceEqual(File.ReadAllBytes(received)),
            $"Snapshot differs. Compare {received} with {approved}.");
        File.Delete(received);
    }

    private static string SnapshotDirectory([CallerFilePath] string sourceFile = "") =>
        Path.Combine(Path.GetDirectoryName(sourceFile)!, "__snapshots__");
}
