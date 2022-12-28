namespace Sample.Components.Services;

public interface IProductValidationService
{
    Task<ProductValidationResult> ValidateProduct(string? sku, string? serialNumber, int location, CancellationToken cancellationToken);
}