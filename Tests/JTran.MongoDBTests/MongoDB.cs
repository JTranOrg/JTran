using MondoCore.Data;
using System.Text.Json.Serialization;
using System.Linq.Expressions;

namespace JTran.MongoDBTests
{
    [TestClass]
    public class MongoDBTests
    {
        private const string ConnectionString = "mongodb://localhost:27017/";
        private const string DatabaseName     = "functionaltests";

        [TestMethod]
        [DataRow("Violet")]
        public void MongoDB_transform(string firstName)
        {
            using var output = File.Open($"c:\\Documents\\Testing\\JTran\\MongoDB\\{firstName}.json", FileMode.Create);
            var transformer  = CreateTransformer(_transformForEach1);
            var db           = new MondoCore.MongoDB.MongoDB(DatabaseName, ConnectionString); 
            var input        = db.GetRepositoryReader<Guid, Person>("persons");
            var enm          = input.AsEnumerable<Person>();
            
            transformer.Transform(enm, output, new TransformerContext { Arguments = new Dictionary<string, object> { { "Name1", firstName}, {"Name2", "" } } } );
        }

        [TestMethod]
        [DataRow("Ezra")]
        public void MongoDB_transform2(string firstName)
        {
            using var output = File.Open($"c:\\Documents\\Testing\\JTran\\MongoDB\\{firstName}.json", FileMode.Create);
            var transformer  = CreateTransformer(_transformForEach2);
            var db           = new MondoCore.MongoDB.MongoDB(DatabaseName, ConnectionString); 
            var input        = db.GetRepositoryReader<Guid, Person>("persons");
            var enm          = input.AsEnumerable<Person>();

            var list = enm.Where( i=> i.FirstName == firstName );

            transformer.Transform(list, output);
        }

        [TestMethod]
        [DataRow("Violet", "Ezra")]
        public async Task MongoDB_transform_back_2_mongo(string firstName1, string firstName2)
        {
            await using var output = new MongoStreamFactory<Person>(DatabaseName, "violets", ConnectionString);
            var transformer  = CreateTransformer(_transformForEach1);
            var db           = new MondoCore.MongoDB.MongoDB(DatabaseName, ConnectionString); 
            var input        = db.GetRepositoryReader<Guid, Person>("persons");
            var enm          = input.AsEnumerable<Person>();

            transformer.Transform(enm, output, new TransformerContext { Arguments = new Dictionary<string, object> { { "Name1", firstName1}, {"Name2", firstName2 }}} );
        }

        private static readonly string _transformForEach1 =
        @"{
            '#foreach(@[FirstName == $Name1 or FirstName == $Name2], [])':
            {
                'Index':                '#(position())',
                'FirstName':            '#(FirstName)',
                'MiddleName':           '#(MiddleName)',
                'Surname':              '#(Surname)',
                'Birthdate':            '#(Birthdate)',
                'Age':                  32.5,
                'Employed':             true,
                'Unemployed':           false,
                'Id':                   null
            }
        }";

        private static readonly string _transformForEach2 =
        @"{
            '#foreach(@, [])':
            {
                '#noobject':  '#copyof(@)',
                'Index':      '#(position())'
            }
        }";

        private static JTran.Transformer CreateTransformer(string transform)
        {
            return new JTran.Transformer(transform, null);
        }

        public class Person : IPartitionable<Guid>
        {
            [JsonPropertyName("id")]
            public Guid     Id    
            {
                get 
                {
                    return _id;
                } 

                set
                {
                    _id = value == Guid.Empty ? Guid.NewGuid() : value;
                }
            }
            public Guid     id    => Id;

            private Guid _id = Guid.NewGuid();

            public string? Surname              { get; set; }
            public string? FirstName            { get; set; }
            public string? MiddleName           { get; set; }
            public string? Birthdate            { get; set; }
            public List<Address>? Addresses     { get; set; }

            public string GetPartitionKey()
            {
                return this.FirstName;
            }
        }

        public class Address
        {
            public string StreetNumber      { get; set; } = "";
            public string StreetName        { get; set; } = "";
            public string City              { get; set; } = "";
            public string State             { get; set; } = "";
            public string ZipCode           { get; set; } = "";
        }
    }
}