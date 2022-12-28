namespace Sample.Warehouse.Api.Controllers;

using Components;
using Components.Services;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;


[ApiController]
[Route("[controller]")]
public class ReceiveController :
    ControllerBase
{
    readonly ILogger<ReceiveController> _logger;
    readonly IProductValidationService _validationService;

    public ReceiveController(ILogger<ReceiveController> logger, IProductValidationService validationService)
    {
        _logger = logger;
        _validationService = validationService;
    }

    /// <summary>
    /// Submit the details of a product received
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="producer">Topic producer for sending event to Kafka</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post(ReceiveDetail detail, [FromServices] ITopicProducer<string, WarehouseEvent> producer)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var validationResult = await _validationService.ValidateProduct(detail.Sku, detail.SerialNumber, detail.Location, HttpContext.RequestAborted);

        if (!validationResult.IsValid)
            return Conflict(new { validationResult.Reason });

        await producer.Produce($"{detail.PurchaseOrderNumber}", new WarehouseEvent
        {
            SourceSystemId = detail.SourceSystemId,
            EventId = detail.EventId,
            Timestamp = detail.Timestamp?.ToString("O"),
            Event = new ProductReceived
            {
                PurchaseOrderNumber = detail.PurchaseOrderNumber,
                Sku = detail.Sku,
                SerialNumber = detail.SerialNumber,
            }
        }, HttpContext.RequestAborted);

        _logger.LogInformation("Received: {PurchaseOrderNumber} ({Sku}) {SerialNumber}", detail.PurchaseOrderNumber, detail.Sku, detail.SerialNumber);

        return Accepted();
    }
}