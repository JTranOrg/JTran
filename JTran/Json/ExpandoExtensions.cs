/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer using an XSLT like language  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: ExpandoExtensions.cs					    		        
 *        Class(es): ExpandoExtensions				         		            
 *          Purpose: Extension methods for ExpandoObject                 
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 25 Apr 2020                                             
 *                                                                          
 *   Copyright (c) 2020-2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{
    /****************************************************************************/
    /****************************************************************************/
    public static class ExpandoExtensions
    {
        /****************************************************************************/
        public static object JsonToExpando(this string s)
        {
            var convertor = new ExpandoObjectConverter();
            var xObject =  JsonConvert.DeserializeObject<ExpandoObject>(s, convertor);
            
            return xObject.SetParent();
        }

        /****************************************************************************/
        internal static ExpandoObject SetParent(this ExpandoObject obj)
        {
            var dict = obj as IDictionary<string, object>;

            foreach(var val in dict)
            {
                if(val.Value != null)
                { 
                    var type = val.Value.GetType().Name;

                    if(!val.Key.StartsWith("_jtran_"))
                    {
                        SetChild(val.Value, obj, null, -1, val.Key);
                    }
                }
            }

            return obj;
        }

        /****************************************************************************/
        public static string ToJson(this ExpandoObject obj)
        {            
            var writer = new JsonStringWriter();

            obj.ToJson(writer);

            return writer.ToString();
        }

        /****************************************************************************/
        internal static void ToJson(this ExpandoObject obj, IJsonWriter writer)
        {
            if(obj == null)
                return;

            writer.StartObject();

            var dict     = (obj as IDictionary<string, object>).Where( kv=> !kv.Key.StartsWith("_jtran_") );
            var numItems = dict.Count();

            foreach(var kv in dict)
            {
                writer.StartChild();
                ToJson(kv.Key, kv.Value, writer);
                writer.EndChild();
            }

            writer.EndObject();
        }

        /****************************************************************************/
        internal static void ChildrenToJson(this ExpandoObject obj, IJsonWriter writer)
        {
            if(obj == null)
                return;

            var dict     = (obj as IDictionary<string, object>).Where( kv=> !kv.Key.StartsWith("_jtran_") );
            var numItems = dict.Count();

            foreach(var kv in dict)
            {
                writer.StartChild();
                ToJson(kv.Key, kv.Value, writer);
                writer.EndChild();
            }
        }

        /****************************************************************************/
        internal static void ToJson(string key, object value, IJsonWriter writer)
        {
            if(value is IEnumerable<object> list)
            {
                writer.WriteContainerName(key);

                writer.WriteList(list);
            }
            else if(value is ExpandoObject expando)
            {
                writer.WriteContainerName(key);
                expando.ToJson(writer);                   
            }
            else
            {                       
                writer.WriteProperty(key, value);
            }
        }

        /****************************************************************************/
        private static void SetChild(object child, object parent, object gparent, int index, string name)
        {
            if(child is ExpandoObject expando)
            {
                dynamic dyn = expando;

                dyn._jtran_parent = parent;

                if(gparent != null)
                    dyn._jtran_gparent = gparent;

                if(index != -1)
                    dyn._jtran_position = index;

                if(name != null)
                    dyn._jtran_name = name;

                expando.SetParent();
            }
            else if(child is IList list)
            {
                var childIndex = 0;

                foreach(var gchild in list)
                    SetChild(gchild, child, parent, childIndex++, (childIndex - 1).ToString());
            }

            return;
        }
    }
}
