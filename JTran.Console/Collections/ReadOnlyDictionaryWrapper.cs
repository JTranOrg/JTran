using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTran.Console.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// A readonly dictionary that wraps a generic IDictionary
    /// </summary>
    internal class ReadOnlyDictionaryWrapper : IReadOnlyDictionary<string, object>
    {
        protected readonly IDictionary<string, object> _dict;

        /****************************************************************************/
        public ReadOnlyDictionaryWrapper(IDictionary<string, object> dict)
        {
            _dict = dict!;
        }

        /****************************************************************************/
        public IEnumerable<string> Keys   => _dict.Keys.ToArray();
        public IEnumerable<object> Values => _dict.Values;
        public int                 Count  => _dict.Count;

        public object this[string key] => _dict[key];

        /****************************************************************************/
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        /****************************************************************************/
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()        
        {
            return _dict.GetEnumerator();
        }

        /****************************************************************************/
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _dict.TryGetValue(key, out value);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
