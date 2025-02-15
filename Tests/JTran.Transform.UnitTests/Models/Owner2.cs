

namespace JTran.Transform.UnitTests
{
    public class Owner2
    {
        public string             Name   { get; set; } = "";
        public List<Automobile2>? Cars   { get; set; }
    }

    public class Automobile3
    {
        public string         Make        { get; set; } = "";
        public string         Model       { get; set; } = "";
        public int            Year        { get; set; }
        public string?        Color       { get; set; }
        public string?        Driver      { get; set; }
        public IList<Driver>? Mechanics   { get; set; }
    }

    public class Automobile4
    {
        public string         Make        { get; set; } = "";
        public string         Model       { get; set; } = "";
        public int            Year        { get; set; }
        public string?        Color       { get; set; }
        public string?        Driver      { get; set; }
        public Engine2?       Engine   { get; set; }
    }

    public class Automobile5
    {
        public string         Make        { get; set; } = "";
        public string         Model       { get; set; } = "";
        public int            Year        { get; set; }
        public string?        Color       { get; set; }
        public string?        Driver      { get; set; }
        public List<Engine2>? Engines     { get; set; }
    }

    public class Engine2
    {
        public double            Displacement   { get; set; }
        public int               Cylinders   { get; set; }
        public string?           Carburation   { get; set; }
    }     

    public class Owner
    {
        public string            Name   { get; set; } = "";
        public List<Automobile3> Cars   { get; set; } = new List<Automobile3>();
    }     

    public class Roster
    {
        public Owner? Owner  { get; set; }
    }

    public class NameValue
    {
        public string? Name  { get; set; }
        public string? Value  { get; set; }
    }
}
