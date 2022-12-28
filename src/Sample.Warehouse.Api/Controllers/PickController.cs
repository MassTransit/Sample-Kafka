using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Sample.Contracts;

namespace Sample.Warehouse.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PickController :
    ControllerBase
{
    readonly ILogger<PickController> _logger;

    public PickController(ILogger<PickController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return NoContent();
    }

    /// <summary>
    /// Submit the details of a product that was picked.
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="producer">Topic producer for sending event to Kafka</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post(PickDetail detail, [FromServices] ITopicProducer<string, WarehouseEvent> producer,
        CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        using var token = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);

        await producer.Produce($"{detail.SourceSystemId}:{detail.OrderNumber}", new WarehouseEvent
        {
            SourceSystemId = detail.SourceSystemId,
            EventId = detail.EventId,
            Timestamp = detail.Timestamp?.ToString("O"),
            Event = new ProductPicked
            {
                OrderNumber = detail.OrderNumber,
                OrderLine = detail.OrderLine!.Value,
                Sku = detail.Sku,
                SerialNumber = detail.SerialNumber,
                LicensePlateNumber = detail.LicensePlateNumber,
            }
        }, token.Token);

        _logger.LogInformation("Produced Pick: {OrderNumber} ({OrderLine})", detail.OrderNumber, detail.OrderLine);

        return Accepted();
    }
}