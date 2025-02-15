using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Common
{
    /****************************************************************************/
    /****************************************************************************/
    public static class ObjectExtensions
    {
        /****************************************************************************/
        /// <summary>
        /// Translates an object into a dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static IDictionary<string, object> ToDictionary(this object obj)
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

        /****************************************************************************/
        public static ICharacterSpan AsCharacterSpan(this object obj, bool cacheable = false)
        {
            if(obj == null)
                return null;

            if(obj is ICharacterSpan cspan)
                return cspan;

            return CharacterSpan.FromString(obj.ToString(), cacheable);
        }

        /****************************************************************************/
        internal static bool TryParseInt(this object obj, out int val)
        {
            val = 0;

            if(obj is null)
                return false;

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
                        
            if(obj is double dbl)
            { 
                val = (int)dbl;
                return true;
            }

            if(obj is float f)
            { 
                val = (int)f;
                return true;
            }

            if(obj is ICharacterSpan cspan && cspan.TryParseNumber(out decimal dval))
            { 
                val = (int)dval;    
                return true;
            }

            if(obj is string str && int.TryParse(str, out int i3))
            { 
                val = i3;    
                return true;
            }

            var t = obj.GetType();

            if(t.IsPrimitive)
            {
                val = (int)Convert.ChangeType(obj, typeof(int));
                return true;
            }

            return false;
        }

        /****************************************************************************/
        internal static bool TryParseDecimal(this object obj, out decimal val)
        {
            val = 0;

            if(obj is null)
                return false;

            if(obj is decimal d)
            { 
                val = d;
                return true;
            }

            if(obj is double dbl)
            { 
                val = (decimal)dbl;
                return true;
            }

            if(obj is float f)
            { 
                val = (decimal)f;
                return true;
            }

            if(obj is int i)
            { 
                val = (decimal)i;
                return true;
            }

            if(obj is long i2)
            { 
                val = (decimal)i2;
                return true;
            }

            if(obj is char ch)
            { 
                val = (decimal)ch;
                return true;
            }

            if(obj is ICharacterSpan cspan && cspan.TryParseNumber(out decimal dval))
            { 
                val = (decimal)dval;    
                return true;
            }

            if(obj is string str && decimal.TryParse(str, out decimal dval2))
            { 
                val = dval2;    
                return true;
            }

            var t = obj.GetType();

            if(t.IsPrimitive)
            {
                val = (decimal)Convert.ChangeType(obj, typeof(decimal));
                return true;
            }

            return false;
        }

        /****************************************************************************/
        internal static bool IsPoco(this object? obj)
        {
            if(obj is null)
                return false;

            if(obj is IObject)
                return false;

            if(obj is IEnumerable)
                return false;

            var t = obj.GetType();

            return !t.IsPrimitive;
        }
    }
}
