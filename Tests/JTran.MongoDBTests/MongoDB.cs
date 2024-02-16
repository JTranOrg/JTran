using MondoCore.Data;
using MondoCore.MongoDB;
using System.Collections;
using System.Text.Json.Serialization;

using JTran;
using MondoCore.Common;

namespace JTran.MongoDBTests
{
    [TestClass]
    public class MongoDBTests
    {
        [TestMethod]
        [DataRow("Violet")]
        public void MongoDB_transform(string firstName)
        {
            using var output = File.Open($"c:\\Documents\\Testing\\JTran\\MongoDB\\{firstName}.json", FileMode.Create);
            var transformer  = CreateTransformer(_transformForEach1);
            var db           = new MondoCore.MongoDB.MongoDB("functionaltests", "mongodb://localhost:27017/"); 
            var input        = db.GetRepositoryReader<Guid, Person>("persons");
            var enm = input.AsEnumerable<Person>();

            transformer.Transform(enm as IEnumerable, output, new TransformerContext { Arguments = (new { Name = firstName }).ToDictionary() } );
        }

        [TestMethod]
        [DataRow("Ezra")]
        public void MongoDB_transform2(string firstName)
        {
            using var output = File.Open($"c:\\Documents\\Testing\\JTran\\MongoDB\\{firstName}.json", FileMode.Create);
            var transformer  = CreateTransformer(_transformForEach2);
            var db           = new MondoCore.MongoDB.MongoDB("functionaltests", "mongodb://localhost:27017/"); 
            var input        = db.GetRepositoryReader<Guid, Person>("persons");
            var enm = input.AsEnumerable<Person>();

            var list = enm.Where( i=> i.FirstName == firstName );

            transformer.Transform(list, output, new TransformerContext { Arguments = (new { Name = firstName }).ToDictionary() } );
        }

        private static readonly string _transformForEach1 =
        @"{
            '#foreach(@[FirstName == $Name], [])':
            {
                'Index':                '#(position())',
                'FirstName':            '#(FirstName)',
                'MiddleName':           '#(MiddleName)',
                'Surname':              '#(Surname)',
                'Birthdate':            '#(Birthdate)'
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
            public Guid     Id    {get; set;}
            public Guid     id    => Id;

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