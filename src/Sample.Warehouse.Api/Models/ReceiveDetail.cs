namespace Sample.Warehouse.Api.Models;

using System.ComponentModel.DataAnnotations;


public record ReceiveDetail
{
    [Required]
    public required string? SourceSystemId { get; init; }

    [Required]
    public required string? EventId { get; init; }

    [Required]
    public required DateTimeOffset? Timestamp { get; init; }

    [Required]
    public required string? PurchaseOrderNumber { get; init; }

    [Required]
    [Range(1, 1000)]
    public required int Location { get; init; }

    [Required]
    public required string? Sku { get; init; }

    [Required]
    public required string? SerialNumber { get; init; }
}