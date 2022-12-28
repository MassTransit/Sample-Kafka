using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Components;
using Sample.Components.Services;
using Sample.Contracts;
using Sample.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureSerilog()
    .UseMassTransit((hostContext, x) =>
    {
        x.AddMongoDbConfiguration(hostContext.Configuration);

        x.TryAddScoped<IProductValidationService, ProductValidationService>();

        x.UsingInMemory();

        x.AddRider(r =>
        {
            r.AddKafkaComponents();

            var location = hostContext.Configuration.GetValue<int>("Warehouse:Location");
            if (location == default)
                throw new ConfigurationException("The warehouse location is required and was not configured.");

            var warehouseTopicName = $"events.warehouse.{location}";

            r.AddProducer<string, WarehouseEvent>(warehouseTopicName, (context, cfg) =>
            {
                // Configure the AVRO serializer, with the schema registry client
                cfg.SetValueSerializer(new AvroSerializer<WarehouseEvent>(context.GetRequiredService<ISchemaRegistryClient>()));
            });

            r.AddProducer<string, ShipmentManifestReceived>("events.erp", (context, cfg) =>
            {
                // Configure the AVRO serializer, with the schema registry client
                cfg.SetValueSerializer(new AvroSerializer<ShipmentManifestReceived>(context.GetRequiredService<ISchemaRegistryClient>()));
            });

            r.UsingKafka((context, cfg) =>
            {
                cfg.ClientId = "WarehouseApi";

                cfg.Host(context);
            });
        });
    });


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();