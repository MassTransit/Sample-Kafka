namespace Sample.Warehouse.Api.Controllers;

using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;


[ApiController]
[Route("[controller]")]
public class ShipController :
    ControllerBase
{
    readonly ILogger<ShipController> _logger;

    public ShipController(ILogger<ShipController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Submit the details of a product received
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="producer">Topic producer for sending event to Kafka</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post(ShipDetail detail, [FromServices] ITopicProducer<string, WarehouseEvent> producer)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        await producer.Produce($"{detail.OrderNumber}", new WarehouseEvent
        {
            SourceSystemId = detail.SourceSystemId,
            EventId = detail.EventId,
            Timestamp = detail.Timestamp?.ToString("O"),
            Event = new ContainerShipped
            {
                OrderNumber = detail.OrderNumber,
                LicensePlateNumber = detail.LicensePlateNumber,
                Carrier = detail.Carrier,
                TrackingNumber = detail.TrackingNumber,
            }
        }, HttpContext.RequestAborted);

        _logger.LogInformation("Shipped: {OrderNumber} ({LicensePlateNumber})", detail.OrderNumber, detail.LicensePlateNumber);

        return Accepted();
    }
}