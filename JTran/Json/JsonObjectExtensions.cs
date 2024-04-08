/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran							            
 *             File: JsonObjectExtensions.cs					    		        
 *        Class(es): JsonObjectExtensions				         		            
 *          Purpose: Extension methods for JsonObject                 
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

using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using JTran.Common;
using JTran.Expressions;
using JTran.Extensions;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class JsonObjectExtensions
    {
        /****************************************************************************/
        internal static object ToJsonObject(this string s)
        {
            using var parser = new Json.Parser(new JsonModelBuilder());

            return parser.Parse(s);
        }

        /****************************************************************************/
        internal static object ToJsonObject(this Stream s)
        {
            using var parser = new Json.Parser(new JsonModelBuilder());

            return parser.Parse(s);
        }

        /****************************************************************************/
        internal static JsonObject JTranToJsonObject(this string s)
        {
            using var parser = new Json.Parser(new JsonModelBuilder());
            
            return parser.Parse(s) as JsonObject;
        }

        /****************************************************************************/
        internal static string ToJson(this JsonObject obj)
        {            
            var writer = new JsonStringWriter();

            obj.ToJson(writer);

            return writer.ToString(); 
        }

        /****************************************************************************/
        internal static void ToJson(this object obj, IJsonWriter writer)
        {
            if(obj == null)
                return;

            writer.StartObject();

            obj.ChildrenToJson(writer);

            writer.EndObject();
        }

        /****************************************************************************/
        internal static void ToJson(ICharacterSpan key, object value, IJsonWriter writer)
        {
            if(value is IEnumerable<object> list)
            {
                writer.WriteContainerName(key);

                writer.WriteList(list);
            }
            else if(value is IObject jobj)
            {
                writer.WriteContainerName(key);
                jobj.ToJson(writer);                   
            }
            else if(value  == null || value is ICharacterSpan|| value is string || !value.GetType().IsClass)
                writer.WriteProperty(key, value);
            else
            {                       
                writer.WriteContainerName(key);
                value.ToJson(writer);                   
            }
        }

        /****************************************************************************/
        internal static void ChildrenToJson(this object obj, IJsonWriter writer)
        {
            if(obj is IObject jobj)
            { 
                jobj.ForEachProperty((name, val)=>
                {
                    writer.StartChild();
                    ToJson(name, val, writer);
                    writer.EndChild();
                });
            }
            else 
            {
                var type = obj.GetType();

                if(!type.IsClass)
                    throw new ArgumentException("Unknown property type");

                var poco = Poco.FromType(type);

                poco.ForEachProperty(obj, (name, value)=> 
                {
                    writer.StartChild();
                    ToJson(name, value, writer); 
                    writer.EndChild();
                });
             }

        }

        /****************************************************************************/
        internal static T ToObject<T>(this JsonObject obj) where T : new()
        {
            return (T)obj.ToObject(typeof(T));
        }

        /****************************************************************************/
        internal static object ToObject(this JsonObject obj, Type t)
        {
            if(obj == null)
                return null;

            var result = Activator.CreateInstance(t);
            var poco = Poco.FromType(t);

            poco.ForEachProperty( (name, prop)=>
            {
                try
                {
                    if(obj.ContainsKey(name))
                    {
                        var val = ToValue(obj[name], prop.PropertyType);

                        prop.SetValue(result, val);
                    }
                }
                catch
                {
                }
            });

            return result;
        }

        /****************************************************************************/
        internal static object ToDictionary(this JsonObject obj, Type t)
        {
            if(t.Name == "IDictionary")
                t = typeof(Dictionary<string, object>);

            var dict = Activator.CreateInstance(t);
            var valType = typeof(object);
            var keyType = typeof(object);
            MethodInfo? add;

            if(t.IsGenericType)
                add = t.GetMethod("Add", new[] { keyType = t.GenericTypeArguments[0], valType = t.GenericTypeArguments[1] });
            else
                add = t.GetMethod("Add", new[] { typeof(object), typeof(object) });

            foreach(var kv in obj) 
            {
                try
                {
                    var val = ToValue(kv.Value, valType);

                    add.Invoke(dict, new object[] { Convert.ChangeType(kv.Key.ToString(), keyType), val });
                }
                catch(Exception ex)
                {
                    var ex2 = ex;
                }
            }

            return dict;
        }

        #region Private 
      
        
        /****************************************************************************/
        private static object ToValue(object val, Type type)
        {
            if(val == null)
                return null;

            if(val.GetType() == type)
                return val;

            if(val is ICharacterSpan cspan && (type == typeof(string) || type == typeof(object)))
                return cspan.ToString();

            if(val is JsonObject exp)
            { 
                if(type.FullName.Contains("Dictionary"))
                    return exp.ToDictionary(type);

                return exp.ToObject(type);
            }

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

            if(type.Name.ToLower() == "string")
                return val.ToString();

            return Convert.ChangeType(val, type);
        }

        /****************************************************************************/
        internal static bool IsList(Type type)
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

        /****************************************************************************/
        internal static Type GetListType(Type type)
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
                    if ("Item" == prop.Name && typeof(object) != prop.PropertyType)
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

        #endregion
    }
}
