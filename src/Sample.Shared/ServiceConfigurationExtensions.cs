using Confluent.Kafka;
using Confluent.SchemaRegistry;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Sample.Shared;

public static class ServiceConfigurationExtensions
{
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
    /// Configure the Kafka Host
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
}