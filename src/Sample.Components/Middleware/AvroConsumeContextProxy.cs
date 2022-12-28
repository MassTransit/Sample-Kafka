namespace Sample.Components.Middleware;

using MassTransit.Context;


public class AvroConsumeContextProxy<TAvro> :
    ConsumeContextProxy
    where TAvro : class
{
    readonly Func<TAvro, object> _propertySelector;

    public AvroConsumeContextProxy(ConsumeContext context, Func<TAvro, object> propertySelector)
        : base(context)
    {
        _propertySelector = propertySelector;
    }

    public override bool TryGetMessage<T>(out ConsumeContext<T> consumeContext)
    {
        if (base.TryGetMessage(out consumeContext))
            return true;

        if (base.TryGetMessage<TAvro>(out ConsumeContext<TAvro>? messageContext))
        {
            var messageProperty = _propertySelector(messageContext.Message);
            if (messageProperty is T message)
            {
                consumeContext = new MessageConsumeContext<T>(this, message);
                return true;
            }
        }

        return false;
    }
}