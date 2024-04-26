namespace Sample.Warehouse.Api.Controllers;

using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;


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

    /// <summary>
    /// Submit the details of a product that was picked.
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="producer">Topic producer for sending event to Kafka</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post(PickDetail detail, [FromServices] ITopicProducer<string, WarehouseEvent> producer)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        await producer.Produce($"{detail.OrderNumber}", new WarehouseEvent
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
                LicensePlateNumber = detail.LicensePlateNumber
            }
        }, HttpContext.RequestAborted);

        _logger.LogInformation("Picked: {OrderNumber} ({OrderLine}) {LicensePlateNumber}", detail.OrderNumber, detail.OrderLine, detail.LicensePlateNumber);

        return Accepted();
    }
}