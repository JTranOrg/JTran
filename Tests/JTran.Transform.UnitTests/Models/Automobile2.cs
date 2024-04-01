

namespace JTran.Transform.UnitTests
{
    public class Automobile2
    {
        public string        FieldName { get; set; } = "";
        public string        Brand     { get; set; } = "";
        public string        Model     { get; set; } = "";
        public int           Year      { get; set; } 
        public string        Color     { get; set; }
        public Engine?       Engine    { get; set; }
        public int?          Index     { get; set; }

        public Driver3?          Driver { get; set; }
    }    
    
    public class Driver3
    {
        public string        FirstName { get; set; } = "";
        public string        LastName  { get; set; } = "";
        public int           Age       { get; set; }
    }

    public class Engine
    {
        public string  Displacement  { get; set; } = "";
    }
}
