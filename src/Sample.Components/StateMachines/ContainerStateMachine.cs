namespace Sample.Components.StateMachines;

using System.Security.Cryptography;
using System.Text;
using Contracts;


public sealed class ContainerStateMachine :
    MassTransitStateMachine<ContainerState>
{
    public ContainerStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ProductPicked, x =>
        {
            x.CorrelateById(context => GenerateIdFromLicensePlateNumber(context.Message.LicensePlateNumber));
            x.ConfigureConsumeTopology = false;
        });

        Event(() => ContainerShipped, x =>
        {
            x.CorrelateById(context => GenerateIdFromLicensePlateNumber(context.Message.LicensePlateNumber));
            x.ConfigureConsumeTopology = false;
        });

        Initially(
            When(ProductPicked)
                .Then(context => context.Saga.Created = DateTime.UtcNow),
            When(ContainerShipped)
                .Then(context => context.Saga.Created = DateTime.UtcNow)
        );

        During(Initial, Picking, Shipped,
            When(ProductPicked)
                .Then(context =>
                {
                    context.Saga.Updated = DateTime.UtcNow;

                    context.Saga.OrderNumber ??= context.Message.OrderNumber;
                    context.Saga.LicensePlateNumber ??= context.Message.LicensePlateNumber;

                    context.Saga.Products ??= new HashSet<ContainerProduct>();

                    context.Saga.Products.Add(new ContainerProduct
                    {
                        Sku = context.Message.Sku,
                        SerialNumber = context.Message.SerialNumber,
                        OrderLine = context.Message.OrderLine
                    });
                })
                .TransitionTo(Picking),
            When(ContainerShipped)
                .Then(context =>
                {
                    context.Saga.Updated = DateTime.UtcNow;

                    context.Saga.OrderNumber ??= context.Message.OrderNumber;
                    context.Saga.LicensePlateNumber ??= context.Message.LicensePlateNumber;

                    context.Saga.Carrier = context.Message.Carrier;
                    context.Saga.TrackingNumber = context.Message.TrackingNumber;
                })
                .TransitionTo(Shipped)
        );
    }

    //
    // ReSharper disable UnassignedGetOnlyAutoProperty
    // ReSharper disable MemberCanBePrivate.Global
    public Event<ProductPicked>? ProductPicked { get; }
    public Event<ContainerShipped>? ContainerShipped { get; }

    public State? Picking { get; }
    public State? Shipped { get; }

    static Guid GenerateIdFromLicensePlateNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentNullException(nameof(number));

        var data = MD5.HashData(Encoding.UTF8.GetBytes(number));

        return new Guid(data);
    }
}