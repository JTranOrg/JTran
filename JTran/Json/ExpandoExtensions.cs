/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{
    /****************************************************************************/
    /****************************************************************************/
    public static class ExpandoExtensions
    {
        /****************************************************************************/
        public static ExpandoObject JsonToExpando(this string s)
        {
            var parser = new Json.Parser(new JsonModelBuilder());
            var exp = parser.Parse(s) as ExpandoObject;

            exp.SetParent();

            return exp;
        }

        /****************************************************************************/
        public static ExpandoObject JTranToExpando(this string s)
        {
            var parser = new Json.Parser(new JsonModelBuilder());
            
            return parser.Parse(s) as ExpandoObject;
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
        public static T ToObject<T>(this ExpandoObject obj) where T : new()
        {
            return (T)obj.ToObject(typeof(T));
        }

        /****************************************************************************/
        public static object ToObject(this ExpandoObject obj, Type t)
        {
            if(obj == null)
                return null;

            var result = Activator.CreateInstance(t);
            var dict = obj as IDictionary<string, object>;

            foreach (PropertyInfo prop in from p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                        where p.CanRead
                                        select p)
            {
                try
                {
                    var name = prop.Name;

                    if(dict.ContainsKey(name))
                    {
                        var val = ToValue(dict[name], prop.PropertyType);

                        prop.SetValue(result, val);
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        #region Private 

        /****************************************************************************/
        private static object ToValue(object val, Type type)
        {
            if(val == null)
                return null;

            if(val.GetType() == type)
                return val;

            if(val is ExpandoObject exp)
                return exp.ToObject(type);

            if(val is IEnumerable<object> array)
            { 
                if(!array.Any() || !IsList(type))
                    return null;

                var listItemType = GetListType(type);
                var listType     = typeof(List<>).MakeGenericType(listItemType);
                var list         = Activator.CreateInstance(listType);
                var addMethod    = list.GetType().GetMethods().FirstOrDefault( m=> m.Name == "Add");

                if(addMethod != null)
                { 
                    foreach(var item in array)
                    {
                        var arrayVal = ToValue(item, listItemType);
                      
                        addMethod.Invoke(list, new object[] { arrayVal });
                    }
                }

                return list;
            }

            return Convert.ChangeType(val, type);
        }

        public static bool IsList(Type type)
        {
            if(null == type)
                throw new ArgumentNullException("type");

            if(typeof(System.Collections.IList).IsAssignableFrom(type))
                return true;

            foreach (var it in type.GetInterfaces())
                if (it.IsGenericType && typeof(IList<>) == it.GetGenericTypeDefinition())
                    return true;

            return false;
        }

        public static Type GetListType(Type type)
        {
            if(type == null)
                throw new ArgumentNullException();

            var etype = typeof(IEnumerable<>);

            foreach (var bt in type.GetInterfaces())
                if (bt.IsGenericType && bt.GetGenericTypeDefinition() == etype)
                    return bt.GetGenericArguments()[0];
            
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
                return typeof(System.Collections.DictionaryEntry);
            
            if (typeof(System.Collections.IList).IsAssignableFrom(type))
            {
                foreach (var prop in type.GetProperties())
                {
                    if ("Item" == prop.Name && typeof(object)!=prop.PropertyType)
                    {
                        var ipa = prop.GetIndexParameters();
                        if (1 == ipa.Length && typeof(int) == ipa[0].ParameterType)
                        {
                            return prop.PropertyType;
                        }
                    }
                }
            }

            if(typeof(System.Collections.ICollection).IsAssignableFrom(type))
            {
                foreach(var addMethods in type.GetMethods().Where( m=> m.Name == "Add"))
                {
                    var parms = addMethods.GetParameters();

                    if (parms.Length == 1 && typeof(object) != parms[0].ParameterType)
                        return parms[0].ParameterType;
                }
            }

            if(typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                return typeof(object);

            return null;
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

        #endregion
    }
}
