namespace ProjectTemplate.Application.Results;

/// <summary>
/// A problem with user-supplied data. Use cases report these instead of throwing;
/// exceptions are reserved for programming errors and broken invariants.
/// </summary>
/// <param name="Code">Stable, dot-separated identifier, for example <c>input.file-not-found</c>.</param>
/// <param name="Message">Human-readable explanation.</param>
public sealed record Issue(string Code, string Message);
