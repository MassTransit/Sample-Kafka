namespace Sample.Components.Services;

public record ProductValidationResult
{
    public bool IsValid { get; init; }
    public string? Reason { get; init; }
}