namespace Sample.Components.Services;

using Contracts;
using MongoDB.Driver;


public class ProductValidationService :
    IProductValidationService
{
    readonly IMongoCollection<ProductLocation> _collection;

    public ProductValidationService(IMongoCollection<ProductLocation> collection)
    {
        _collection = collection;
    }

    public async Task<ProductValidationResult> ValidateProduct(string? sku, string? serialNumber, int location, CancellationToken cancellationToken)
    {
        using var sessionHandle = await _collection.Database.Client.StartSessionAsync(cancellationToken: cancellationToken);

        FilterDefinition<ProductLocation>? filter = Builders<ProductLocation>.Filter.And(
            Builders<ProductLocation>.Filter.Eq(x => x.Sku, sku),
            Builders<ProductLocation>.Filter.Eq(x => x.SerialNumber, serialNumber));

        IAsyncCursor<ProductLocation> asyncCursor = await _collection.FindAsync<ProductLocation>(sessionHandle, filter, null, cancellationToken);
        var productLocation = await asyncCursor.FirstAsync(cancellationToken);

        if (productLocation.Location == location)
            return new ProductValidationResult { IsValid = true };

        return new ProductValidationResult
        {
            IsValid = false,
            Reason = $"Product expected at location {productLocation.Location}"
        };
    }
}