namespace Sample.Components.Consumers;

using Contracts;
using MongoDB.Driver;


public class ShipmentManifestReceivedConsumer :
    IConsumer<ShipmentManifestReceived>
{
    readonly IMongoCollection<ProductLocation> _collection;

    public ShipmentManifestReceivedConsumer(IMongoCollection<ProductLocation> collection)
    {
        _collection = collection;
    }

    public async Task Consume(ConsumeContext<ShipmentManifestReceived> context)
    {
        using var sessionHandle = await _collection.Database.Client.StartSessionAsync(cancellationToken: context.CancellationToken);

        sessionHandle.StartTransaction();

        foreach (var product in context.Message.Items)
        {
            FilterDefinition<ProductLocation>? filter = Builders<ProductLocation>.Filter.And(
                Builders<ProductLocation>.Filter.Eq(x => x.Sku, product.Sku),
                Builders<ProductLocation>.Filter.Eq(x => x.SerialNumber, product.SerialNumber));

            await _collection.ReplaceOneAsync(sessionHandle, filter, new ProductLocation
            {
                Sku = product.Sku,
                SerialNumber = product.SerialNumber,
                Location = context.Message.DeliveryLocation
            }, new ReplaceOptions { IsUpsert = true });
        }

        await sessionHandle.CommitTransactionAsync(context.CancellationToken);
    }
}