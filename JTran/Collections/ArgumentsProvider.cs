/***************************************************************************
 *                                                                          
 *    JTran - A JSON to JSON transformer  							                    
 *                                                                          
 *        Namespace: JTran.Collections						            
 *             File: ArgumentsProvider.cs					    		        
 *        Class(es): ArgumentsProvider				         		            
 *          Purpose: A list of dictionaries to provide arguments              
 *                                                                          
 *  Original Author: Jim Lightfoot                                          
 *    Creation Date: 1 Oct 2024                                             
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
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    public class ArgumentsProvider : ReadonlyDictionary<string, object>
    {
        private readonly List<IReadOnlyDictionary<string, object>> _providers = new();

        /****************************************************************************/
        public ArgumentsProvider()
        {
        }

        /****************************************************************************/
        public void Add(IReadOnlyDictionary<string, object> provider)
        {
            try
            {
                // If the provider supports ContainsKey then add it directly
                provider.ContainsKey("test");
                _providers.Add(provider);
            }
            catch
            {
                // Otherwise add a wrapper
                _providers.Add(new Arguments(provider));
            }

        }

        /****************************************************************************/
        public override object this[string key] 
        { 
            get
            {
                if(this.TryGetValue(key, out object val))
                { 
                    return val;
                }

                throw new KeyNotFoundException();
            }
        }

        /****************************************************************************/
        public override bool ContainsKey(string key)
        {
            foreach(var provider in _providers)
                if(provider.ContainsKey(key))
                    return true;

            return false;
        }

        /****************************************************************************/
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            foreach(var provider in _providers)
                if(provider.TryGetValue(key, out value))
                    return true;

            value = null;
            return false;
        }

        /****************************************************************************/
        public override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if(!_providers.Any())
                return Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator();

            return _providers.First().GetEnumerator();
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    file class Arguments(IReadOnlyDictionary<string, object> provider) : ReadonlyDictionary<string, object> 
    {
        /****************************************************************************/
        public override object this[string key]  => provider[key];

        /****************************************************************************/
        public override bool ContainsKey(string key) => true;

        /****************************************************************************/
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return provider.TryGetValue(key, out value);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    internal class ReadOnlyDictionaryWrapper(IDictionary<string, object> provider) : ReadonlyDictionary<string, object> 
    {
        /****************************************************************************/
        public override object this[string key]  => provider[key];

        /****************************************************************************/
        public override bool ContainsKey(string key) => provider.ContainsKey(key);

        /****************************************************************************/
        public override bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return provider.TryGetValue(key, out value);
        }
    }

    /****************************************************************************/
    /****************************************************************************/
    public abstract class ReadonlyDictionary<K, V> : IReadOnlyDictionary<K, V>
    { 
        /****************************************************************************/
        public abstract V this[K key]  { get; }
        public abstract bool ContainsKey(K key);         
        public abstract bool TryGetValue(K key, [MaybeNullWhen(false)] out V value);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #region NotSupported

        public virtual IEnumerable<K> Keys => throw new NotSupportedException();
        public virtual IEnumerable<V> Values => throw new NotSupportedException();
        public virtual int Count => throw new NotSupportedException();

        #endregion
    }

}
