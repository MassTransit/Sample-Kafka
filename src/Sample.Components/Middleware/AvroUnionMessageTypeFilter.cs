namespace Sample.Components.Middleware;

using Avro.Specific;


public class AvroUnionMessageTypeFilter<TAvro> :
    IFilter<ConsumeContext>
    where TAvro : class, ISpecificRecord
{
    readonly Func<TAvro, object> _propertySelector;

    public AvroUnionMessageTypeFilter(Func<TAvro, object> propertySelector)
    {
        _propertySelector = propertySelector;
    }

    public Task Send(ConsumeContext context, IPipe<ConsumeContext> next)
    {
        var proxy = new AvroConsumeContextProxy<TAvro>(context, _propertySelector);

        return next.Send(proxy);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("avroFilter");
    }
}