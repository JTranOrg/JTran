
using JTran.Common;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Dynamic;
using System.IO;

namespace JTran.Json
{
   
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model
    /// </summary>
    internal class LegacyParser : IJsonParser
    {
        /****************************************************************************/
        internal LegacyParser()
        { 
        }

        /****************************************************************************/
        public ExpandoObject Parse(Stream stream) 
        {
            var data      = stream.ReadString();
            var convertor = new ExpandoObjectConverter();
            var xObject   =  JsonConvert.DeserializeObject<ExpandoObject>(data, convertor);
            
            return xObject.SetParent();
        }
    }
}
