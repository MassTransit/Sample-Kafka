namespace Sample.Components.StateMachines;

public class ContainerState :
    SagaStateMachineInstance,
    ISagaVersion
{
    public string? CurrentState { get; set; }

    public string? OrderNumber { get; set; }
    public string? LicensePlateNumber { get; set; }
    public string? InvoiceNumber { get; set; }

    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public HashSet<ContainerProduct>? Products { get; set; }

    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }

    public int Version { get; set; }

    public Guid CorrelationId { get; set; }
}