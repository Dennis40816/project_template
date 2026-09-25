using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectTemplate.Application.Inspection;
using ProjectTemplate.Application.Results;
using ProjectTemplate.Domain;

namespace ProjectTemplate.Desktop.Features.Inspect;

/// <summary>
/// UI state and commands only. The fingerprint is computed by <see cref="InspectFileUseCase"/>,
/// never here, so Desktop and CLI cannot disagree.
/// </summary>
public sealed partial class InspectViewModel : ObservableObject
{
    private readonly InspectFileUseCase _inspectFile;

    public InspectViewModel(InspectFileUseCase inspectFile)
    {
        ArgumentNullException.ThrowIfNull(inspectFile);
        _inspectFile = inspectFile;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(InspectCommand))]
    public partial string Path { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? LengthText { get; set; }

    [ObservableProperty]
    public partial string? Sha256 { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    private bool CanInspect() => !string.IsNullOrWhiteSpace(Path);

    [RelayCommand(CanExecute = nameof(CanInspect))]
    private async Task InspectAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = null;
        LengthText = null;
        Sha256 = null;

        Result<FileFingerprint> result = await _inspectFile.ExecuteAsync(Path, cancellationToken);
        if (result.IsSuccess && result.Value is { } fingerprint)
        {
            LengthText = string.Create(CultureInfo.InvariantCulture, $"{fingerprint.Length:N0} bytes");
            Sha256 = fingerprint.Sha256;
            return;
        }

        ErrorMessage = string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message));
    }
}
