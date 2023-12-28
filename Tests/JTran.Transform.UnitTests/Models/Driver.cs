

namespace JTran.Transform.UnitTests
{
    public class DriverContainer
    {
        public Driver? Driver { get; set; }
    }

    public class DriverContainer2
    {
        public Driver2? Driver { get; set; }
    }

    public class Driver
    {
        public string       FieldName { get; set; } = "";
        public string       FirstName { get; set; } = "";
        public string       LastName  { get; set; } = "";
        public Automobile2? Car       { get; set; }
    }

    public class Driver2
    {
        public string FirstName         { get; set; } = "";
        public string LastName          { get; set; } = "";
        public string Engine            { get; set; } = "";
        public List<Automobile2> Vehicles { get; set; }
        public string OriginalDriver    { get; set; } = "";
        public string Mechanics         { get; set; } = "";
        public string TrackNo           { get; set; } = "";
        public string TrackName         { get; set; } = "";
        public string Uncle             { get; set; } = "";
        public string Aunt              { get; set; } = "";
        public string Cousin            { get; set; } = "";
    }

}
