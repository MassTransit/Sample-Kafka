namespace Sample.Components.Middleware;

using Avro.Specific;
using MassTransit.Configuration;


public class AvroUnionMessageTypeFilterPipeSpecification<TAvro> :
    IPipeSpecification<ConsumeContext>
    where TAvro : class, ISpecificRecord
{
    readonly Func<TAvro, object> _propertySelector;

    public AvroUnionMessageTypeFilterPipeSpecification(Func<TAvro, object> propertySelector)
    {
        _propertySelector = propertySelector;
    }

    public void Apply(IPipeBuilder<ConsumeContext> builder)
    {
        builder.AddFilter(new AvroUnionMessageTypeFilter<TAvro>(_propertySelector));
    }

    public IEnumerable<ValidationResult> Validate()
    {
        yield break;
    }
}