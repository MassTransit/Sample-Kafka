namespace Sample.Components;

using Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;


public static class SampleMongoDbConfigurationExtensions
{
    public static T AddMongoDbConfiguration<T>(this T configurator, IConfiguration configuration)
        where T : IServiceCollection
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(ProductLocation)))
        {
            BsonClassMap.RegisterClassMap(new BsonClassMap<ProductLocation>(cfg =>
            {
                cfg.AutoMap();
                cfg.SetIgnoreExtraElements(true);
            }));
        }

        configurator.TryAddSingleton<IMongoClient>(_ => new MongoClient(configuration.GetConnectionString("MongoDb")));

        configurator.TryAddSingleton<IMongoCollection<ProductLocation>>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();

            return client.GetDatabase("KafkaDemo").GetCollection<ProductLocation>("product.location");
        });

        return configurator;
    }
}