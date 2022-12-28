namespace Sample.Warehouse.Api.Models;

using System.ComponentModel.DataAnnotations;


public record ProductDetail
{
    [Required]
    [MinLength(6)]
    public required string? Sku { get; init; }

    [Required]
    [MinLength(6)]
    public required string? SerialNumber { get; init; }
}