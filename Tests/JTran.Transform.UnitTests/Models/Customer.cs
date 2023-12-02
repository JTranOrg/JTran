

namespace JTran.Transform.UnitTests
{
    public class Customer
    {
        public string FirstName   { get; set; } = "";
        public string LastName    { get; set; } = "";
        public int    Age         { get; set; }
        public string Address     { get; set; } = "";
    } 

    public class CustomerContainer
    {
        public string          SpecialCustomer    { get; set; } = "";
        public List<Customer>? Customers          { get; set; }
    }     
}
