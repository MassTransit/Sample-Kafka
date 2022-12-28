namespace Sample.Warehouse.Api.Controllers;

using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models;


[ApiController]
[Route("[controller]")]
public class ManifestController :
    ControllerBase
{
    readonly ILogger<ManifestController> _logger;

    public ManifestController(ILogger<ManifestController> logger)
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
    public async Task<IActionResult> Post(ManifestDetail detail, [FromServices] ITopicProducer<string, ShipmentManifestReceived> producer)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        await producer.Produce($"{detail.PurchaseOrderNumber}", new ShipmentManifestReceived
        {
            SourceSystemId = detail.SourceSystemId,
            EventId = detail.EventId,
            Timestamp = detail.Timestamp?.ToString("O"),
            PurchaseOrderNumber = detail.PurchaseOrderNumber,
            DeliveryLocation = detail.DeliveryLocation,
            Items = new List<Product>(detail.Products?.Select(x => new Product
            {
                Sku = x.Sku,
                SerialNumber = x.SerialNumber
            }) ?? Enumerable.Empty<Product>())
        }, HttpContext.RequestAborted);

        _logger.LogInformation("Manifest: {PurchaseOrderNumber} ({DeliveryLocation})", detail.PurchaseOrderNumber, detail.DeliveryLocation);

        return Accepted();
    }
}