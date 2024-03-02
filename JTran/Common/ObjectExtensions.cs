using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JTran.Common
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Translates an object into a dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null)
                return null;

            if (obj is IDictionary<string, object> dict)
                return dict;

            if(obj is IDictionary dict2)
            {
                var result = new Dictionary<string, object>();

                foreach(var key in dict2.Keys)
                    result.Add(key.ToString(), dict2![key]);

                return result;
            }
            
            if(obj is IEnumerable list)
            {
                var result = new Dictionary<string, object>();

                foreach(var val in list)
                    result.Add(val.ToString(), val);

                return result;
            }

             return Poco.FromObject(obj).ToDictionary(obj);
        }

        internal static CharacterSpan AsCharacterSpan(this object obj)
        {
            if(obj is CharacterSpan cspan)
                return cspan;

            return CharacterSpan.FromString(obj.ToString());
        }

        internal static bool TryParseInt(this object obj, out int val)
        {
            val = 0;

            if(obj is decimal d)
            { 
                val = (int)d;
                return true;
            }

            if(obj is int i)
            { 
                val = i;
                return true;
            }

            if(obj is long i2)
            { 
                val = (int)i2;
                return true;
            }

            return false;
        }
    }
}
