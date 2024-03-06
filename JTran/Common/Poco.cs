/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran.Common							            
 *             File: Poco.cs					    		        
 *        Class(es): Poco				         		            
 *          Purpose: Class to represent a POCO                
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 29 Feb 2024                                             
 *                                                                          
 *   Copyright (c) 2024 - Jim Lightfoot, All rights reserved           
 *                                                                          
 *  Licensed under the MIT license:                                         
 *    http://www.opensource.org/licenses/mit-license.php                    
 *                                                                          
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JTran.Common
{
    /****************************************************************************/
    /****************************************************************************/
    internal class Poco
    {
        private static readonly Dictionary<string, Poco> _cache = new();
        private readonly Dictionary<ICharacterSpan, PropertyInfo> _properties = new();

        /****************************************************************************/
        internal Poco(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where( p=> p.CanRead );

            foreach(var property in properties ) 
            {
                _properties.Add(CharacterSpan.FromString(property.Name), property);
            }
        }

        /****************************************************************************/
        public static Poco FromObject(object obj)
        {
            return FromType(obj.GetType());
        }

        /****************************************************************************/
        public static Poco FromType(Type type)
        {
            var key = type.FullName;

            if(_cache.ContainsKey(key))
                return _cache[key];

            var poco = new Poco(type);

            _cache[key] = poco;

            return poco;
        }

        /****************************************************************************/
        public object? GetValue(object? poco, ICharacterSpan name)  
        {
            if(poco == null ) 
                return null;

            if(!_properties.ContainsKey(name)) 
                return null;

            var property = _properties[name];

            return property.GetValue(poco);
        }

        /****************************************************************************/
        public bool IsEmpty(object poco)  
        {
            foreach(var property in _properties.Values)
            {
                try
                { 
                    var value = property.GetValue(poco, null);

                    if (value != null)
                        return false;
                }
                catch
                {
                    // Just ignore it
                }
            } 

            return true;
        }

        /****************************************************************************/
        public void ForEachProperty(object poco, Action<ICharacterSpan, object> onProperty)  
        {        
            foreach(var kv in _properties)
            {
                try
                { 
                    var value = kv.Value.GetValue(poco, null);

                    onProperty(kv.Key, value);
                }
                catch
                {
                    // Just ignore it
                }
            }
        }

        /****************************************************************************/
        public IDictionary<string, object> ToDictionary(object poco)  
        {
            var result = new Dictionary<string, object>();

            foreach(var property in _properties.Values)
            {
                try
                { 
                    var value = property.GetValue(poco, null);

                    // Add property name and value to dictionary
                    if (value != null)
                        result.Add(property.Name, value);
                }
                catch
                {
                    // Just ignore it
                }
            } 
            
            return result;
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class PocoObject : IObject
    {
        private readonly Poco _poco;

        internal object? Data { get; set; }

        /****************************************************************************/
        internal PocoObject(Poco poco)
        {
            _poco = poco;
        }

        /****************************************************************************/
        public object? GetPropertyValue(ICharacterSpan name)
        {
            return _poco.GetValue(this.Data, name);
        }
    }
}
