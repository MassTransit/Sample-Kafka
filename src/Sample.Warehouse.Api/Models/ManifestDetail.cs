namespace Sample.Warehouse.Api.Models;

using System.ComponentModel.DataAnnotations;


public record ManifestDetail
{
    [Required]
    [MinLength(6)]
    public required string? SourceSystemId { get; init; }

    [Required]
    [MinLength(24)]
    public required string? EventId { get; init; }

    [Required]
    public required DateTimeOffset? Timestamp { get; init; }

    [Required]
    [MinLength(6)]
    public required string? PurchaseOrderNumber { get; init; }

    [Required]
    [Range(1, 1000)]
    public required int DeliveryLocation { get; init; }

    [Required]
    public List<ProductDetail>? Products { get; init; }
}