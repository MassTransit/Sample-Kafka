namespace Sample.Shared;

using Confluent.Kafka;
using Confluent.SchemaRegistry;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;


public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Configure the Confluent Schema Registry for connection to Confluent Cloud
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AddKafkaComponents<T>(this T services)
        where T : IServiceCollection
    {
        services.AddOptions<KafkaOptions>().BindConfiguration("Kafka");
        services.AddOptions<SchemaRegistryOptions>().BindConfiguration("SchemaRegistry");

        services.AddSingleton<ISchemaRegistryClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<SchemaRegistryOptions>>().Value;

            return new CachedSchemaRegistryClient(new Dictionary<string, string>
            {
                { "schema.registry.url", options.Server! },
                { "schema.registry.basic.auth.credentials.source", "SASL_INHERIT" },
                { "sasl.username", options.Username! },
                { "sasl.password", options.Password! }
            });
        });

        return services;
    }

    /// <summary>
    /// Just an example, but some retry/kill switch combination to stop processing when the consumer/saga faults repeatedly.
    /// </summary>
    /// <param name="configurator"></param>
    public static void UseSampleRetryConfiguration(this IKafkaTopicReceiveEndpointConfigurator configurator)
    {
        configurator.UseKillSwitch(k => k.SetActivationThreshold(1).SetRestartTimeout(m: 1).SetTripThreshold(0.2).SetTrackingPeriod(m: 1));
        configurator.UseMessageRetry(retry => retry.Interval(1000, TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// Configure the Kafka Host using SASL_SSL to connect to Confluent Cloud
    /// </summary>
    /// <param name="configurator"></param>
    /// <param name="context"></param>
    public static void Host(this IKafkaFactoryConfigurator configurator, IRiderRegistrationContext context)
    {
        var options = context.GetRequiredService<IOptions<KafkaOptions>>().Value;

        configurator.Host(options.Servers, h =>
        {
            h.UseSasl(s =>
            {
                s.SecurityProtocol = SecurityProtocol.SaslSsl;
                s.Mechanism = SaslMechanism.Plain;
                s.Username = options.Username;
                s.Password = options.Password;
            });
        });
    }

    public static T ConfigureSerilog<T>(this T builder)
        where T : IHostBuilder
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        builder.UseSerilog();

        return builder;
    }
}