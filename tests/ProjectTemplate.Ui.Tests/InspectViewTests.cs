using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using ProjectTemplate.Bootstrap;
using ProjectTemplate.Desktop;
using ProjectTemplate.Desktop.Shell;

namespace ProjectTemplate.Ui.Tests;

[Trait("Category", "ui")]
public sealed class InspectViewTests
{
    [AvaloniaFact]
    public async Task InspectingAFileShowsItsFingerprint()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.bin");
        await File.WriteAllBytesAsync(path, "abc"u8.ToArray(), TestContext.Current.CancellationToken);
        try
        {
            MainWindow window = App.CreateMainWindow(CompositionRoot.Create());
            window.Show();

            Find<TextBox>(window, "PathBox").Text = path;
            Button button = Find<Button>(window, "InspectButton");
            Assert.True(button.Command!.CanExecute(null));
            button.Command.Execute(null);

            await WaitUntilAsync(() => !string.IsNullOrEmpty(Find<SelectableTextBlock>(window, "Sha256Text").Text));

            Assert.Equal(
                "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                Find<SelectableTextBlock>(window, "Sha256Text").Text);
            Assert.Equal("3 bytes", Find<TextBlock>(window, "LengthText").Text);
            window.Close();
        }
        finally
        {
            File.Delete(path);
        }
    }

    [AvaloniaFact]
    public void InspectIsDisabledWithoutAPath()
    {
        MainWindow window = App.CreateMainWindow(CompositionRoot.Create());
        window.Show();

        Assert.False(Find<Button>(window, "InspectButton").Command!.CanExecute(null));
        window.Close();
    }

    // Feature views have their own name scope, so search the logical tree instead of FindControl.
    private static T Find<T>(Control root, string name)
        where T : Control =>
        root.GetLogicalDescendants().OfType<T>().Single(control => control.Name == name);

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (int attempt = 0; attempt < 200 && !condition(); attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        Assert.True(condition(), "The UI did not reach the expected state within 2 seconds.");
    }
}
