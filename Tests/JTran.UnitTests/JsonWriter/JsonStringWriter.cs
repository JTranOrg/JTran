using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json.Linq;

using JTran.Expressions;
using JTran.Json;

using System.Text.Json;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("JsonWriter")]
    public class JsonStringWriterTests
    {
        [TestMethod]
        public void JsonStringWriter_WriteItem()
        {
            var writer = new JsonStringWriter();

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

            writer.WriteItem(ship, true);

            var result = writer.ToString();
            var shipJson = JsonSerializer.Serialize(ship);
   
            Assert.IsTrue(JToken.DeepEquals(JObject.Parse(result), JObject.Parse(shipJson)));
        }
    }
}
