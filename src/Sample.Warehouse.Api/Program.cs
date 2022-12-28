using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using MassTransit;
using Sample.Contracts;
using Sample.Shared;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseSerilog()
    .UseMassTransit((hostContext, x) =>
    {
        x.UsingInMemory();

        x.AddRider(r =>
        {
            r.AddKafkaComponents();

            var topicName = hostContext.Configuration.GetValue<string>("Topics:WarehouseEvent");
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ConfigurationException("The topicName was not configured: WarehouseEvent");

            r.AddProducer<string, WarehouseEvent>(topicName, (context, cfg) =>
            {
                // Configure the AVRO serializer, with the schema registry client
                cfg.SetValueSerializer(new AvroSerializer<WarehouseEvent>(context.GetService<ISchemaRegistryClient>()));
            });


            r.UsingKafka((context, cfg) => cfg.Host(context));
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