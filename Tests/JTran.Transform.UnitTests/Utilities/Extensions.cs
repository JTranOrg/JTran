using System.Collections;
using System.Reflection;

using Newtonsoft.Json.Linq;

namespace JTran.Transform.UnitTests
{
    internal static class Extensions
    {
        internal static JToken? PathValue(this JObject obj, string path)
        {
            var parts = path.Split(".");
            JToken? token = obj;

            foreach(var part in parts)
            {
                if (token == null)
                    return null;

                token = token[part];
            }

            return token;
        }

        internal static T? PathValue<T>(this JObject obj, string path)
        {
            var token = obj.PathValue(path);

            if(token == null)
                return default(T?);

            return (T)Convert.ChangeType(token.ToString(), typeof(T));
        }
    }
}
