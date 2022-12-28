namespace Sample.Warehouse.Api.Models;

using System.ComponentModel.DataAnnotations;


public record PickDetail
{
    [Required]
    public required string? SourceSystemId { get; init; }

    [Required]
    public required string? EventId { get; init; }

    [Required]
    public DateTimeOffset? Timestamp { get; init; }

    [Required]
    public required string? OrderNumber { get; init; }

    [Required]
    public required long? OrderLine { get; init; }

    [Required]
    public required string? Sku { get; init; }

    [Required]
    public required string? SerialNumber { get; init; }

    [Required]
    public required string? LicensePlateNumber { get; init; }
}