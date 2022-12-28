namespace Sample.Warehouse.Api.Controllers;

public record PickDetail
{
    public required string? SourceSystemId { get; init; }

    public required string? EventId { get; init; }
    public DateTimeOffset? Timestamp { get; init; }

    public required string? OrderNumber { get; init; }
    public required long? OrderLine { get; init; }

    public required string? Sku { get; init; }
    public required string? SerialNumber { get; init; }
    public required string? LicensePlateNumber { get; init; }
}