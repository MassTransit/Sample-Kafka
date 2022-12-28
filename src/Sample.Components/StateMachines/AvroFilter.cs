using MassTransit;

namespace Sample.Components.StateMachines;

public class AvroFilter<TAvro> :
    IFilter<ConsumeContext>
    where TAvro : class
{
    readonly Func<TAvro, object> _propertySelector;

    public AvroFilter(Func<TAvro, object> propertySelector)
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