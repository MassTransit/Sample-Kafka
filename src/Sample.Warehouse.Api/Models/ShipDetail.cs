namespace Sample.Warehouse.Api.Models;

using System.ComponentModel.DataAnnotations;


public record ShipDetail
{
    [Required]
    public required string? SourceSystemId { get; init; }

    [Required]
    public required string? EventId { get; init; }

    [Required]
    public required DateTimeOffset? Timestamp { get; init; }

    [Required]
    public required string? OrderNumber { get; init; }

    [Required]
    public required string? LicensePlateNumber { get; init; }

    [Required]
    public required string? Carrier { get; init; }

    [Required]
    public required string? TrackingNumber { get; init; }
}