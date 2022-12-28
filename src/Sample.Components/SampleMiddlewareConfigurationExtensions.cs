namespace Sample.Components;

using Avro.Specific;
using Middleware;


public static class SampleMiddlewareConfigurationExtensions
{
    public static void UseAvroUnionMessageTypeFilter<TAvro>(this IConsumePipeConfigurator configurator, Func<TAvro, object> propertySelector)
        where TAvro : class, ISpecificRecord
    {
        configurator.AddPrePipeSpecification(new AvroUnionMessageTypeFilterPipeSpecification<TAvro>(propertySelector));
    }
}