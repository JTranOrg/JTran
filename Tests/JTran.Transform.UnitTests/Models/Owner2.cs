

namespace JTran.Transform.UnitTests
{
    public class Owner2
    {
        public string             Name   { get; set; } = "";
        public List<Automobile2>? Cars   { get; set; }
    }

    public class Automobile3
    {
        public string        Make       { get; set; } = "";
        public string        Model      { get; set; } = "";
        public int           Year       { get; set; }
        public string        Color      { get; set; } = "";
        public IList<Driver> Mechanics  { get; set; } = new List<Driver>();
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
}
