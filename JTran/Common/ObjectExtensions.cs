using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            var result = new Dictionary<string, object>();

            if(obj is IDictionary dict2)
            {
                foreach(var key in dict2.Keys)
                    result.Add(key.ToString(), dict2![key]);
            }
            else if(obj is IEnumerable list)
            {
                foreach(var val in list)
                    result.Add(val.ToString(), val);
            }
            else
            {
            // ??? create cache, class
                // Get public and instance properties only
                var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where( p=> p.CanRead );

                foreach(var property in properties)
                {
                    try
                    { 
                        var value = property.GetGetMethod().Invoke(obj, null);

                        // Add property name and value to dictionary
                        if (value != null)
                            result.Add(property.Name, value);
                    }
                    catch
                    {
                        // Just ignore it
                    }
                }
            }

            return result;
        }
    }
}
