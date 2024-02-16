using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTran.UnitTests
{
    internal class Shipment
    {
        public string Id           { get; set; } = "";
        public int    Index        { get; set; }
        public string Description  { get; set; } = "";
        public int    Quantity     { get; set; }
        public double Width        { get; set; }
        public double Height       { get; set; }
        public double Volume       { get; set; }
        public double Weight       { get; set; }
    }

    internal class Container
    {
        public string          Id           { get; set; } = "";
        public int             Index        { get; set; }
        public string          Size         { get; set; } = "";
        public List<Shipment>? Shipments    { get; set; }
    }

    internal class Pod
    {
        public string           Id           { get; set; } = "";
        public int              Index        { get; set; }
        public string           Origin       { get; set; } = "";
        public string           Destination  { get; set; } = "";
        public List<Container>? Containers   { get; set; }
    }

    internal class Ship
    {
        public string     Name             { get; set; } = "";
        public string     Class            { get; set; } = "";
        public string     Registration     { get; set; } = "";
        public double     YearBuilt        { get; set; }
        public double     Length           { get; set; }
        public double     Width            { get; set; }
        public double     Height           { get; set; }
        public int        NumEngines       { get; set; } = 2;
        public double     MaxWarp          { get; set; } = 9.5;
        public string     PodAttachment    { get; set; } = "";
        public int        MaxPods          { get; set; } = 20;
        public List<Pod>? Pods             { get; set; }
    }
}
