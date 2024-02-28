using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using JTran.Common;

using System.Text.Json;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonWriter")]
    public class JsonStreamWriterTests
    {
        [TestMethod]
        public void JsonStreamWriter_WriteItem()
        {
            var ship = new 
            {
                Name  = "Rocinante",
                Class = "Heavy Freighter",
            };

            using var output = new MemoryStream();

            using(var writer = new JsonStreamWriter(output))
            { 
                writer.WriteItem(ship, true);
            }

            output.Seek(output.Position, SeekOrigin.Begin);

            var result = output.ReadString();
            var shipJson = JsonSerializer.Serialize(ship);
   
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(result), JObject.Parse(shipJson)));
        }

        [TestMethod]
        public void JsonStreamWriter_WriteItem2()
        {
            var ship = new Ship
            {
                Name              = "Rocinante",
                Class             = "Heavy Freighter",
                Registration      = "Rigel 5",
                YearBuilt         = 2465,

                Pods = new List<Pod>
                {
                    new Pod 
                    {
                        Id          = Guid.NewGuid().ToString(),
                        Index       = 1,
                        Origin      = "Terra",
                        Destination = "Rigel 3",
                        Containers  = new List<Container> 
                        {
                            new Container
                            {
                                Id     = Guid.NewGuid().ToString(),
                                Index  = 1,
                                Size   = "Full",
                                Shipments = new List<Shipment> 
                                {
                                    new Shipment
                                    { 
                                        Id          = Guid.NewGuid().ToString(), 
                                        Index       = 1,
                                        Description = "Medical Supplies",
                                        Quantity    = 32
                                    },
                                    new Shipment
                                    { 
                                        Id          = Guid.NewGuid().ToString(), 
                                        Index       = 1,
                                        Description = "Drones",
                                        Quantity    = 4
                                    }
                                }
                            }
                        }
                    }
                }
            };

            using var output = new MemoryStream();

            using(var writer = new JsonStreamWriter(output))
            { 
                writer.WriteItem(ship, true);
            }

            output.Seek(output.Position, SeekOrigin.Begin);

            var result = output.ReadString();
            var shipJson = JsonSerializer.Serialize(ship);
   
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(result), JObject.Parse(shipJson)));
        }
    }
}
