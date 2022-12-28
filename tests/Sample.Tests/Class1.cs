using Avro.IO;
using Avro.Specific;
using NUnit.Framework;
using Sample.Contracts;

namespace Sample.Tests;

[TestFixture]
public class Class1
{
    [Test]
    public void Thing()
    {
        var picked = new ProductPicked
        {
            Sku = "1234",
            SerialNumber = "21-213-72",
            OrderNumber = "12345",
        };

        var warehouseEvent = new WarehouseEvent
        {
            Event = picked
        };

        var bytes = Serialize(warehouseEvent);

        var result = Deserialize<WarehouseEvent>(bytes);
    }

    [Test]
    public void Method()
    {
        var type = typeof(IConsumer<Union<WarehouseEvent, ProductPicked>>);
    
        // Message<ProductPicked>()
        //     .MapFrom<WarehouseEvent>(x => x.Event);
        
    }
    
    // if WarehouseEvent has a single `object` property, that's the ticket
    // if a properly IsAssignableFrom(T) then that's it
    

    public interface Union<T, TP>
        where T : class
        where TP : class
    {
    }

    public interface IConsumer<T>
        where T : class
    {
    }

    public static byte[] Serialize<T>(T thisObj) where T : ISpecificRecord
    {
        using var ms = new MemoryStream();
        var enc = new BinaryEncoder(ms);
        var writer = new SpecificDefaultWriter(thisObj.Schema); // Schema comes from pre-compiled, code-gen phase
        writer.Write(thisObj, enc);
        return ms.ToArray();
    }

    public static T Deserialize<T>(byte[] bytes) where T : ISpecificRecord, new()
    {
        using var ms = new MemoryStream(bytes);
        var dec = new BinaryDecoder(ms);
        var regenObj = new T();

        var reader = new SpecificDefaultReader(regenObj.Schema, regenObj.Schema);
        reader.Read(regenObj, dec);
        return regenObj;
    }
}