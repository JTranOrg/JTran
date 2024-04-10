
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JTran.Collections
{
    /****************************************************************************/
    /****************************************************************************/
    internal class RestrictedAccessDictionary : IReadOnlyDictionary<string, object> 
    {
        private readonly Dictionary<string, object> _contents = new();

        /****************************************************************************/
        internal RestrictedAccessDictionary()
        {
        }

        /****************************************************************************/
        public object              this[string key] => _contents[key];
        public IEnumerable<string> Keys        => _contents.Keys;
        public IEnumerable<object> Values      => _contents.Values;
        public int                 Count       => _contents.Count;

        /****************************************************************************/
        public bool ContainsKey(string key)
        {
            return _contents.ContainsKey(key);
        }

        /****************************************************************************/
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _contents.GetEnumerator();
        }

        /****************************************************************************/
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            return _contents.TryGetValue(key, out value);
        }

        /****************************************************************************/
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _contents.GetEnumerator();
        }

        /****************************************************************************/
        internal void SetValue(string key, object val)
        {
            _contents[key] = val;
        }
    }
}
